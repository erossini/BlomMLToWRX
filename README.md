# BlomMLToWRX

The tool can be used if you need to convert BlogML (for example export from Blogengine.net) format into WRX file (WordPress Extended RSS import file format) for subsequent import it into WordPress.

The code in this repository has error handling/logging enhancements and fixed import of multiple comments.

For detailed description see the blog post [""](http://puresourcecode.com/)

### Parameters

Options available with the tool.  
  * RemoveComments  
  * ExportToWRX  
  * QATarget  
  * QASource  
  * NewWRXWithOnlyFailedPosts
  * SourceImageUrl
  * DestinationImageUrl

### Usage

You simply run the tool with the following command

```
BlogMLMLCore.exe 
    /Action:ExportToWRX 
    /BlogMLFile:BlogML.xml 
    /SourceUrl:http://puresourcecode.com 
    /TargetUrl:https://www.puresourcecode.com 
    /SourceImageUrl:http://puresourcecode.com/javascript/image.axd?picture=
    /DestinationImageUrl:https://www.puresourcecode.com/myimg/javascript/
```
