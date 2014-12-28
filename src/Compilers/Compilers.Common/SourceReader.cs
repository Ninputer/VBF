// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace VBF.Compilers
{
    public class SourceReader
    {
        public const int DefaultTabSize = 1;

        //reverting service
        private StringBuilderReader m_backupReader;
        private StringBuilder m_backupStreamBuilder;
        private SourceLocation m_lastLocation;
        private SourceLocation m_location;
        private int m_revertPointKeySeed;
        private RevertPointCollection m_revertPoints;
        private TextReader m_textReader;

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
        public int TabSize { get; private set; }

        public bool IsEndOfStream
        {
            get
            {
                return PeekChar() < 0;
            }
        }

        public SourceLocation Location
        {
            get
            {
                return m_lastLocation;
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
                //backup stream is empty
                if (m_revertPoints.Count == 0)
                {
                    DeleteBackupStream();
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
