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

using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class EndOfStreamParser : Parser<Lexeme>
    {
        public override ParserFunc<TFuture> BuildParser<TFuture>(Future<Lexeme, TFuture> future)
        {
            ParserFunc<TFuture> scan = null;
            scan = (scanner, context) =>
            {

                var l = scanner.Read();

                if (l.IsEndOfStream)
                {
                    return context.StepResult(0, () => future(l)(scanner, context));
                }
                ErrorCorrection deleteCorrection = new DeletedErrorCorrection(l);
                return context.StepResult(1, () => scan(scanner, context), deleteCorrection); //delete to recover
            };

            return scan;
        }
    }
}
