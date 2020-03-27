using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BlogMLConverter.Utilities
{
    /// <summary>
    /// Class ConsoleCopy.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class ConsoleCopy : IDisposable
    {

        /// <summary>
        /// The file stream
        /// </summary>
        FileStream fileStream;
        /// <summary>
        /// The file writer
        /// </summary>
        StreamWriter fileWriter;
        /// <summary>
        /// The double writer
        /// </summary>
        TextWriter doubleWriter;
        /// <summary>
        /// The old out
        /// </summary>
        TextWriter oldOut;

        /// <summary>
        /// Class DoubleWriter.
        /// Implements the <see cref="System.IO.TextWriter" />
        /// </summary>
        /// <seealso cref="System.IO.TextWriter" />
        class DoubleWriter : TextWriter
        {
            /// <summary>
            /// The one
            /// </summary>
            TextWriter one;
            /// <summary>
            /// The two
            /// </summary>
            TextWriter two;

            /// <summary>
            /// Initializes a new instance of the <see cref="DoubleWriter"/> class.
            /// </summary>
            /// <param name="one">The one.</param>
            /// <param name="two">The two.</param>
            public DoubleWriter(TextWriter one, TextWriter two)
            {
                this.one = one;
                this.two = two;
            }

            /// <summary>
            /// When overridden in a derived class, returns the character encoding in which the output is written.
            /// </summary>
            /// <value>The encoding.</value>
            public override Encoding Encoding
            {
                get { return one.Encoding; }
            }

            /// <summary>
            /// Clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
            /// </summary>
            public override void Flush()
            {
                one.Flush();
                two.Flush();
            }

            /// <summary>
            /// Writes a character to the text stream.
            /// </summary>
            /// <param name="value">The character to write to the text stream.</param>
            public override void Write(char value)
            {
                one.Write(value);
                two.Write(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleCopy"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public ConsoleCopy(string path)
        {
            oldOut = Console.Out;

            try
            {
                fileStream = File.Create(path);

                fileWriter = new StreamWriter(fileStream);
                fileWriter.AutoFlush = true;

                doubleWriter = new DoubleWriter(fileWriter, oldOut);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open file for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(doubleWriter);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Console.SetOut(oldOut);
            if (fileWriter != null)
            {
                fileWriter.Flush();
                fileWriter.Close();
                fileWriter = null;
            }
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }
        }
    }
}