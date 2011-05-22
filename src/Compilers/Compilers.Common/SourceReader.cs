using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace VBF.Compilers
{
    public class SourceReader
    {
        public const int DefaultTabSize = 4;

        private TextReader m_textReader;

        //scanner location service
        private SourceLocation m_location;
        private SourceLocation m_lastLocation;

        //reverting service
        private StringBuilderReader m_backupReader;
        private StringBuilder m_backupStreamBuilder;
        private int m_revertPointKeySeed = 0;
        private RevertPointCollection m_revertPoints;

        public int TabSize { get; private set; }

        public SourceReader(TextReader textReader, int tabSize)
        {
            CodeContract.RequiresArgumentNotNull(textReader, "textReader");
            m_textReader = textReader;

            //start counting line number from 1
            m_location.Line = 1;
            m_lastLocation.Line = 1;

            TabSize = tabSize;
        }

        public SourceReader(TextReader textReader) : this(textReader, DefaultTabSize) { }

        public bool IsEndOfStream
        {
            get
            {
                return PeekChar() < 0;
            }
        }

        public int PeekChar()
        {
            if (m_backupStreamBuilder != null)
            {
                int charValue = m_backupReader.Peek();

                if (charValue >= 0)
                {
                    return charValue;
                }
                else
                {
                    //backup stream is empty
                    if (m_revertPoints.Count == 0)
                    {
                        DeleteBackupStream();
                    }
                }
            }

            return m_textReader.Peek();
        }

        public SourceLocation PeekLocation()
        {
            return m_location;
        }

        public int ReadChar()
        {
            int charValue = InternalReadChar();

            //save last location
            m_lastLocation = m_location;

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

                    m_location.Line += 1;
                    m_location.Column = 0;

                }

            }

            return charValue;
        }

        public SourceLocation Location
        {
            get
            {
                return m_lastLocation;
            }
        }

        public RevertPoint CreateRevertPoint()
        {
            if (m_backupStreamBuilder == null)
            {
                CreateBackupStream();
            }

            int revertPointKey = GetRevertPointKey();

            RevertPoint revertPoint = new RevertPoint(revertPointKey, m_backupReader.Position, m_lastLocation, m_location);
            m_revertPoints.Add(revertPoint);

            return revertPoint;
        }

        public void Revert(RevertPoint revertPoint)
        {
            CodeContract.Requires(m_revertPoints.Contains(revertPoint.Key), "revertPoint",
                "The revert point is invalid");
            Debug.Assert(revertPoint.Offset >= 0 && revertPoint.Offset < m_backupStreamBuilder.Length);

            //seek backup stream reader
            m_backupReader.Position = revertPoint.Offset;

            //restore location state
            m_lastLocation = revertPoint.LastLocation;
            m_location = revertPoint.Location;
        }

        public void RemoveRevertPoint(RevertPoint revertPoint)
        {
            m_revertPoints.Remove(revertPoint.Key);
        }

        private void CreateBackupStream()
        {
            m_backupStreamBuilder = new StringBuilder();
            m_backupReader = new StringBuilderReader(m_backupStreamBuilder);

            if (m_revertPoints == null)
            {
                m_revertPoints = new RevertPointCollection();
            }
        }

        private void DeleteBackupStream()
        {
            if (m_backupReader != null)
            {
                m_backupReader.Close();
            }
            m_backupReader = null;
            m_backupStreamBuilder = null;
        }

        private int GetRevertPointKey()
        {
            return m_revertPointKeySeed++;
        }

        private int InternalReadChar()
        {
            if (m_backupStreamBuilder != null)
            {
                return ReadBackupStream();
            }
            return m_textReader.Read();
        }

        private int ReadBackupStream()
        {
            int charValue = m_backupReader.Read();
            if (charValue < 0)
            {

                //check if backing up necessory
                if (m_revertPoints.Count == 0)
                {
                    DeleteBackupStream();
                    return m_textReader.Read();
                }

                //read from the original text reader
                charValue = m_textReader.Read();

                if (charValue < 0)
                {
                    //original reader ended
                    return charValue;
                }

                //backup the value
                m_backupStreamBuilder.Append((char)charValue);
                m_backupReader.Position += 1;
            }

            return charValue;
        }
    }
}
