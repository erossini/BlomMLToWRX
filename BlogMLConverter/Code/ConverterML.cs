using BlogML.Core.Xml;
using BlogMLConverter.Extensions;
using BlogMLConverter.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BlogMLConverter.Code
{
    public class ConverterML
    {
        #region variables
        ConsoleCopy consoleCopy;

        string _originalFilePath = @"C:\Users\enric\Downloads\BlogMLNews.xml";
        string _destinationFilePath = "";

        string _originalUrl = "http://puresourcecode.com/";
        string _destinationUrl = "https://www.puresourcecode.com/";

        string _originalImageUrl = "http://puresourcecode.com/javascript/image.axd?picture=";
        string _destinationImageUrl = "https://www.puresourcecode.com/myimg/javascript/";

        XNamespace blogmlNS = "http://www.blogml.com/2006/09/BlogML";

        public ConverterML() { }

        public ConverterML(ConsoleCopy consoleCopy, string originalFilePath, string destinationFilePath, string originalPath, string replacePath)
        {
            this.consoleCopy = consoleCopy;
            _originalFilePath = originalFilePath;
            _destinationFilePath = destinationFilePath;
            _originalUrl = originalPath;
            _destinationUrl = replacePath;
        }
        #endregion

        public void RemoveAllComments(string fileName)
        {
            try
            {
                XDocument xd = XDocument.Load(fileName);
                var posts = from c in xd.Descendants(blogmlNS + "post") select c;

                foreach (var post in posts)
                {
                    var comments = post.Element(blogmlNS + "comments");
                    if (comments != null)
                    {
                        string postName = post.Element(blogmlNS + "post-name").Value;
                        var commentsCount = from r in comments.Descendants(blogmlNS + "comment")
                                            select r;

                        Console.WriteLine("{0}. {1}. REMOVED", commentsCount.Count(), postName);
                        comments.Remove();
                    }
                }

                xd.Save(fileName);
            }
            catch (Exception ex)
            {
                throw (new Exception(String.Format("An error occurred. {0}", ex)));
            }
        }

        public void QATarget(string fileName)
        {
            string qaReportFileName = string.Format("{0}.Report.txt", Path.GetFileNameWithoutExtension(fileName));
            StreamWriter swQACheck = File.CreateText(qaReportFileName);

            foreach (var url in File.ReadAllLines(fileName))
            {
                string format;
                try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                    {
                        format = string.Format("{0},{1}", response.StatusCode, url);
                    }
                }
                catch (WebException ex)
                {
                    format = string.Format("{0},{1}", ex.Status, url);
                }

                Console.WriteLine(format);
                swQACheck.WriteLine(format);
            }
            swQACheck.Close();
        }

        public void GenerateHelperFiles(string sourceBaseUrl, string targetBaseurl, string wrxFilePath)
        {
            if (string.IsNullOrEmpty(sourceBaseUrl))
                throw new ArgumentNullException("sourceBaseUrl", "Source Base URL is mandatory parameter");
            if (string.IsNullOrEmpty(targetBaseurl))
                throw new ArgumentNullException("targetBaseurl", "Target Base URL is mandatory parameter");
            if (string.IsNullOrEmpty(wrxFilePath))
                throw new ArgumentNullException("wrxFilePath", "WRX File Path is mandatory parameter");

            //Delete files if already exists
            string redirectFilePath = string.Format("{0}.Redirect.txt", Path.GetFileNameWithoutExtension(wrxFilePath));
            string sourceQAFilePath = string.Format("{0}.SourceQA.txt", Path.GetFileNameWithoutExtension(wrxFilePath));
            string targetQAFilePath = string.Format("{0}.TargetQA.txt", Path.GetFileNameWithoutExtension(wrxFilePath));

            if (File.Exists(redirectFilePath))
                File.Delete(redirectFilePath);

            if (File.Exists(sourceQAFilePath))
                File.Delete(sourceQAFilePath);

            if (File.Exists(targetQAFilePath))
                File.Delete(targetQAFilePath);

            StreamWriter swRedirect = File.CreateText(redirectFilePath);
            StreamWriter swSourceQA = File.CreateText(sourceQAFilePath);
            StreamWriter swTargetQA = File.CreateText(targetQAFilePath);

            //Redirect File
            string redirectFormat = "RewriteRule ^{0}$ http://{1}/{2} [R=301, NC, L]";
            string sourceQAFormat = "http://{0}{1}";
            string targetQAFormat = "http://{0}/{1}";

            XDocument xd = XDocument.Load(wrxFilePath);
            XNamespace wp = "http://wordpress.org/export/1.0/";
            var posts = from c in xd.Descendants("item") select c;

            foreach (var post in posts)
            {
                string link = post.Element("link").Value;
                string postName = post.Element(wp + "post_name").Value;

                string r = string.Format(redirectFormat, link, targetBaseurl, postName);
                swRedirect.WriteLine(r);

                string s = string.Format(sourceQAFormat, sourceBaseUrl, link);
                swSourceQA.WriteLine(s);

                string t = string.Format(targetQAFormat, targetBaseurl, postName);
                swTargetQA.WriteLine(t);
            }

            //Close all streams
            swRedirect.Close();
            swSourceQA.Close();
            swTargetQA.Close();
        }

        public string GenerateWRXFile()
        {
            if (string.IsNullOrEmpty(_originalFilePath))
                throw new ArgumentNullException("blogMLFilePath", "BlogMLFilePath is mandatory parameter");

            if (string.IsNullOrEmpty(_destinationFilePath))
                _destinationFilePath = string.Format("{0}.WRX.xml", Path.GetFileNameWithoutExtension(_originalFilePath));

            BlogMLBlog blogML = SerializeBlogML(_originalFilePath);
            WriteWXRDocument(blogML, string.Empty, _destinationFilePath);

            return _destinationFilePath;
        }

        public string GenerateWRXFileWithFailedPosts(string wrxFileName, string qaReportFileName)
        {
            List<string> lines = File.ReadAllLines(qaReportFileName).ToList();
            List<string> errorPostNames = new List<string>();
            foreach (string line in lines)
            {
                string status = line.Split(',')[0];
                string url = line.Split(',')[1];

                if (status != "OK")
                {
                    string[] urlsplit = url.Split('/');
                    string postname = urlsplit[urlsplit.Length - 1];

                    errorPostNames.Add(postname);
                }
            }

            XDocument xd = XDocument.Load(wrxFileName);
            XNamespace wp = "http://wordpress.org/export/1.0/";

            var posts = (from c in xd.Descendants("item") select c).ToList();

            foreach (var post in posts)
            {
                var element = post.Element(wp + "post_name");
                if (element != null)
                {
                    string postname = element.Value;

                    var r = from s in errorPostNames
                            where s == postname
                            select s;

                    if (!r.Any())
                        post.Remove();
                }
            }

            string wrxNewFileName = string.Format("{0}.OnlyFailed.xml", Path.GetFileNameWithoutExtension(wrxFileName));
            xd.Save(wrxNewFileName);

            return wrxNewFileName;
        }

        public BlogMLBlog SerializeBlogML(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(BlogMLBlog));

            TextReader reader = new StreamReader(fileName);
            BlogMLBlog blogData = (BlogMLBlog)serializer.Deserialize(reader);
            reader.Close();

            return blogData;
        }

        public void WriteWXRDocument(BlogMLBlog blogData, string baseUrl, string fileName)
        {
            XmlTextWriter writer = new XmlTextWriter(fileName, new UTF8Encoding(false));
            writer.Formatting = Formatting.Indented;

            writer.WriteStartDocument();
            writer.WriteStartElement("rss");
            writer.WriteAttributeString("version", "2.0");
            writer.WriteAttributeString("xmlns", "content", null, "http://purl.org/rss/1.0/modules/content/");
            writer.WriteAttributeString("xmlns", "wfw", null, "http://wellformedweb.org/CommentAPI/");
            writer.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
            writer.WriteAttributeString("xmlns", "wp", null, "http://wordpress.org/export/1.0/");

            // Write Blog Info.
            writer.WriteStartElement("channel");
            writer.WriteElementString("title", String.Join(" ", blogData.Title));
            writer.WriteElementString("link", baseUrl + blogData.RootUrl);
            writer.WriteElementString("description", "Exported Blog");
            writer.WriteElementString("pubDate", blogData.DateCreated.ToString("ddd, dd MMM yyyy HH:mm:ss +0000"));
            writer.WriteElementString("generator", "http://wordpress.org/?v=MU");
            writer.WriteElementString("language", "en");
            writer.WriteElementString("wp:wxr_version", "1.0");
            writer.WriteElementString("wp:base_site_url", blogData.RootUrl);
            writer.WriteElementString("wp:base_blog_url", blogData.RootUrl);

            // Create tags (currently not in use with BlogML document)
            //for(int i = 0; i <= tagCount - 1; i++)
            //{
            //    writer.WriteStartElement("wp:tag");
            //    writer.WriteElementString("wp:tag_slug", tags[0].ToString().Replace(' ', '-'));
            //    writer.WriteStartElement("wp:tag_name");
            //    writer.WriteCData(tags[0].ToString());
            //    writer.WriteEndElement(); // wp:tag_name
            //    writer.WriteEndElement(); // sp:tag
            //}

            // Create categories
            if (blogData.categories != null)
            {
                BlogMLCategory currCategory = null;
                for (int i = 0; i <= blogData.categories.Count - 1; i++)
                {
                    currCategory = blogData.categories[i];
                    writer.WriteStartElement("wp:category");
                    writer.WriteElementString("wp:category_nicename", string.Join(" ", currCategory.Title).ToLower().Replace(' ', '-'));
                    writer.WriteElementString("wp:category_parent", "");
                    writer.WriteStartElement("wp:cat_name");
                    writer.WriteCData(string.Join(" ", currCategory.Title));
                    writer.WriteEndElement(); // wp:cat_name
                    writer.WriteEndElement(); // wp:category
                }
            }

            // TODO: Swap code so that all posts are processed, not just first 5.
            for (int i = 0; i <= blogData.Posts.Count - 1; i++)
            {
                string postXml = WritePost(blogData.Posts[i], blogData, baseUrl);
                writer.WriteRaw(postXml);
            }

            writer.WriteEndElement(); // channel
            writer.WriteEndElement(); // rss

            writer.Flush();
            writer.Close();
        }

        private string WritePost(BlogMLPost currPost, BlogMLBlog blogData, string baseUrl)
        {
            try
            {
                var memoryStream = new MemoryStream();
                var writer = new XmlTextWriter(memoryStream, Encoding.Unicode);
                writer.Formatting = Formatting.Indented;
                BlogMLCategoryReference currCatRef;
                string categoryName;
                BlogMLComment currComment;

                writer.WriteStartElement("item");
                writer.WriteElementString("title", string.Join(" ", currPost.Title));
                writer.WriteElementString("link", baseUrl + currPost.PostUrl);
                writer.WriteElementString("pubDate", currPost.DateCreated.ToString("ddd, dd MMM yyyy HH:mm:ss +0000"));
                writer.WriteStartElement("dc:creator");
                writer.WriteCData(String.Join(" ", currPost.Authors));
                writer.WriteEndElement(); // dc:creator

                // Post Tags/Categories (currently only categories are implemented with BlogML
                if (currPost.Categories != null)
                {
                    for (int j = 0; j <= currPost.Categories.Count - 1; j++)
                    {
                        currCatRef = currPost.Categories[j];
                        categoryName = GetCategoryById(blogData, currCatRef.Ref);
                        writer.WriteStartElement("category");
                        writer.WriteCData(categoryName);
                        writer.WriteEndElement(); // category
                        writer.WriteStartElement("category");
                        writer.WriteAttributeString("domain", "category");
                        writer.WriteAttributeString("nicename", categoryName.ToLower().Replace(' ', '-'));
                        writer.WriteCData(categoryName);
                        writer.WriteEndElement(); // category domain=category
                    }
                }

                writer.WriteStartElement("guid");
                writer.WriteAttributeString("isPermaLink", "false");
                writer.WriteString(" ");
                writer.WriteEndElement(); // guid
                writer.WriteElementString("description", ".");
                writer.WriteStartElement("content:encoded");
                var content = currPost.Content.Text;
                var startCdata = "<![CDATA[";//"&amp;lt;![CDATA[";
                if (!String.IsNullOrEmpty(content) && content.Contains(startCdata))
                {
                    // content = content.Replace(startCdata, "<!-- &amp;lt;![CDATA[ -->");
                    // content = content.Replace("]]&amp;", "<!-- ]]&amp; -->");
                    content = content.Replace(startCdata, "<!-- <![CDATA-[ -->");
                    content = content.Replace("]]>", "<!-- ]]-> -->");
                    content = content.Replace(_originalImageUrl, _destinationImageUrl);
                    Console.WriteLine("In post " + currPost.ID + " " + currPost.PostUrl + " replaced  <![CDATA[  and ]]> with comment ");
                }
                writer.WriteCData(content);
                writer.WriteEndElement(); // content:encoded
                writer.WriteElementString("wp:post_id", currPost.ID);
                writer.WriteElementString("wp:post_date", currPost.DateCreated.ToString("yyyy-MM-dd HH:mm:ss"));
                writer.WriteElementString("wp:post_date_gmt", currPost.DateCreated.ToString("yyyy-MM-dd HH:mm:ss"));
                writer.WriteElementString("wp:comment_status", "open");
                writer.WriteElementString("wp:ping_status", "open");
                writer.WriteElementString("wp:post_name", string.Join(" ", currPost.Title).SafeUrl());
                writer.WriteElementString("wp:status", "publish");
                writer.WriteElementString("wp:post_parent", "0");
                writer.WriteElementString("wp:menu_order", "0");
                writer.WriteElementString("wp:post_type", "post");
                //writer.WriteStartElement("wp:post_password");
                //writer.WriteString(" ");
                //writer.WriteEndElement(); // wp:post_password

                if (currPost.Comments != null)
                {
                    for (int k = 0; k <= currPost.Comments.Count - 1; k++)
                    {
                        currComment = currPost.Comments[k];
                        writer.WriteStartElement("wp:comment");
                        // currComment.id="http://geekswithblogs.net/mnf/archive/2016/02/17/scrolltocontrol-helper-method-for-asp.net-web-forms-to-move-position.aspx#648420";
                        //extract after #
                        var commentId = currComment.ID.RightAfter("#");
                        writer.WriteElementString("wp:comment_id", commentId);
                        writer.WriteElementString("wp:comment_date", currComment.DateCreated.ToString("yyyy-MM-dd HH:mm:ss"));
                        writer.WriteElementString("wp:comment_date_gmt", currComment.DateCreated.ToString("yyyy-MM-dd HH:mm:ss"));
                        writer.WriteStartElement("wp:comment_author");
                        if ((!String.IsNullOrEmpty(currComment.UserEMail)) || (currComment.UserEMail != "http://"))
                        {
                            writer.WriteCData(currComment.UserName);
                        }
                        else
                        {
                            writer.WriteCData("Nobody");
                        }
                        writer.WriteEndElement(); // wp:comment_author
                        writer.WriteElementString("wp:comment_author_email", currComment.UserEMail);
                        writer.WriteElementString("wp:comment_author_url", currComment.UserUrl);
                        writer.WriteElementString("wp:comment_type", " ");
                        writer.WriteStartElement("wp:comment_content");
                        var commentContent = currComment.Content.Text;
                        //Import stripped  <p> and <br> html tags in comments, but keeps new lines  
                        commentContent = commentContent.Replace("</p>", Environment.NewLine);
                        commentContent = commentContent.Replace("<br />", Environment.NewLine);
                        writer.WriteCData(commentContent);
                        writer.WriteEndElement(); // wp:comment_content

                        if (currComment.Approved)
                        {
                            writer.WriteElementString("wp:comment_approved", null, "1");
                        }
                        else
                        {
                            writer.WriteElementString("wp:comment_approved", null, "0");
                        }

                        //writer.WriteElementString("wp", "comment_parent", null, "0");
                        writer.WriteElementString("wp:comment_parent", null, "0");
                        writer.WriteEndElement(); // wp:comment
                    }
                }

                writer.WriteEndElement(); // item
                writer.Flush();

                //http://stackoverflow.com/questions/78181/how-do-you-get-a-string-from-a-memorystream
                memoryStream.Position = 0;
                var sr = new StreamReader(memoryStream);
                var xmlString = sr.ReadToEnd();
                return xmlString;
            }
            catch (Exception exc)
            {
                Console.WriteLine("Trying to save " + currPost.ID + " " + currPost.PostUrl + " error: " + exc.ToString());
                return "";
            }
        }

        private string GetCategoryById(BlogMLBlog BlogData, string CategoryId)
        {
            string results = "none";
            bool found = false;
            //try to find by ID
            for (int i = 0; i <= BlogData.categories.Count - 1; i++)
            {
                if (BlogData.categories[i].ID == CategoryId)
                {
                    results = String.Join(" ", BlogData.categories[i].Title);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                //try to find by description
                for (int i = 0; i <= BlogData.categories.Count - 1; i++)
                {
                    if (BlogData.categories[i].Description == CategoryId)
                    {
                        results = String.Join(" ", BlogData.categories[i].Title);
                        found = true;
                        break;
                    }
                }
            }
            Debug.Assert(found, CategoryId);
            return results;
        }
    }
}
