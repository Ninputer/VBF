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

namespace VBF.Compilers
{
    public class CompilationErrorInfo
    {
        public CompilationErrorInfo(int id, int level, CompilationStage stage, string messageTemplate)
        {
            Id = id;
            Level = level;
            Stage = stage;
            MessageTemplate = messageTemplate;
        }

        public int Id { get; private set; }
        public int Level { get; private set; }
        public CompilationStage Stage { get; private set; }
        public string MessageTemplate { get; set; }
    }
}
