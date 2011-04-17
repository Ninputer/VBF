using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VBF.Compilers.Scanners
{
    public class SourceCode
    {
        public int DefaultTabSize = 4;

        private TextReader m_textReader;
        private bool m_isLastCharLf;
        private bool m_peekCharCacheHasValue;
        private int m_peekCharCache;
        private SourceLocation m_location;

        public int TabSize { get; set; }

        public SourceCode(TextReader textReader)
        {
            CodeContract.RequiresArgumentNotNull(textReader, "textReader");
            m_textReader = textReader;

            TabSize = DefaultTabSize;
        }

        public bool IsEndOfStream
        {
            get
            {
                return PeekChar() < 0;
            }
        }

        public int PeekChar()
        {

            if (!m_peekCharCacheHasValue)
            {
                m_peekCharCache = m_textReader.Read();
                m_peekCharCacheHasValue = true;
            }
            return m_peekCharCache;
        }

        public int ReadChar()
        {
            int charValue;
            if (m_peekCharCacheHasValue)
            {
                charValue = m_peekCharCache;
                m_peekCharCacheHasValue = false;
            }
            else
            {
                charValue = m_textReader.Read();
            }

            if (charValue >= 0)
            {
                Char c = (char)charValue;

                //increase index
                m_location.CharIndex += 1;

                //increase column
                if (c == '\t')
                {
                    m_location.Column += TabSize;
                }
                else
                {
                    m_location.Column += 1;
                }

                //increase line if necessory
                //treat \n as the line breaker. \r\n will be handled too
                if (c == '\n')
                {
                    m_isLastCharLf = true;
                }
                else
                {
                    if (m_isLastCharLf)
                    {
                        m_location.Line += 1;
                        m_location.Column = 0;

                        m_isLastCharLf = false;
                    }
                }
                
            }

            return charValue;
        }

        public SourceLocation Location
        {
            get
            {
                return m_location;
            }
        }
    }
}
