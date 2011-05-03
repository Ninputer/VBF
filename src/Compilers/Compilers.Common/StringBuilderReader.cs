using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VBF.Compilers
{

    class StringBuilderReader : TextReader
    {
        private int m_Position;

        private StringBuilder m_sb;

        public StringBuilderReader(StringBuilder s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            m_sb = s;
        }

        public override void Close()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_sb = null;
                m_Position = 0;
            }

            base.Dispose(disposing);
        }

        public override int Peek()
        {
            if (m_sb == null)
            {
                throw new ObjectDisposedException(null, "The StringBuilderReader has been closed");
            }
            if (m_Position == m_sb.Length)
            {
                return -1;
            }
            return m_sb[m_Position];
        }

        public override int Read()
        {
            if (m_sb == null)
            {
                throw new ObjectDisposedException(null, "The StringBuilderReader has been closed");
            }
            if (m_Position == m_sb.Length)
            {
                return -1;
            }
            return m_sb[m_Position++];
        }

        public override int Read(char[] buffer, int index, int count)
        {

            CodeContract.RequiresArgumentNotNull(buffer, "buffer");
            CodeContract.RequiresArgumentInRange(index >= 0, "index", "The index must be greater or equal to zero");
            CodeContract.RequiresArgumentInRange(count >= 0, "count", "The index must be greater or equal to zero");

            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException("There's no enough buffer for this index and count");
            }
            if (m_sb == null)
            {
                throw new ObjectDisposedException(null, "The StringBuilderReader has been closed");
            }

            int leftLength = m_sb.Length - m_Position;
            if (leftLength > 0)
            {
                if (leftLength > count)
                {
                    leftLength = count;
                }
                m_sb.CopyTo(m_Position, buffer, index, leftLength);
                m_Position += leftLength;
            }
            return leftLength;
        }

        public override string ReadLine()
        {
            if (m_sb == null)
            {
                throw new ObjectDisposedException(null, "The StringBuilderReader has been closed");
            }
            int i = m_Position;
            while (i < m_sb.Length)
            {
                char ch = m_sb[i];
                switch (ch)
                {
                    case '\r':
                    case '\n':
                        {
                            string line = m_sb.ToString(m_Position, i - m_Position);
                            m_Position = i + 1;
                            if (((ch == '\r') && (m_Position < m_sb.Length)) && (m_sb[m_Position] == '\n'))
                            {
                                m_Position++;
                            }
                            return line;
                        }
                }
                i++;
            }
            if (i > m_Position)
            {
                string lineToEnd = m_sb.ToString(m_Position, i - m_Position);
                m_Position = i;
                return lineToEnd;
            }
            return null;
        }

        public override string ReadToEnd()
        {
            string str;
            if (m_sb == null)
            {
                throw new ObjectDisposedException(null, "The StringBuilderReader has been closed");
            }
            if (m_Position == 0)
            {
                str = m_sb.ToString();
            }
            else
            {
                str = m_sb.ToString(m_Position, m_sb.Length - m_Position);
            }
            m_Position = m_sb.Length;
            return str;
        }

        public int Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                CodeContract.RequiresArgumentInRange(value >= 0 && value <= m_sb.Length, "value", "Invalid offset");
                m_Position = value;
            }
        }
    }


}
