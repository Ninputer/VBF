using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers
{
    public class CompilationErrorList : IReadOnlyList<CompilationError>
    {
        private List<CompilationError> m_errors;
        private CompilationErrorManager m_errorManager;

        public void AddError(int id, SourceSpan errorPosition, params object[] args)
        {
            CodeContract.RequiresArgumentInRange(m_errorManager.ContainsErrorDefinition(id), "id", "Error id is invalid");

            var errorInfo = m_errorManager.GetErrorInfo(id);
            var errorMessage = String.Format(errorInfo.MessageTemplate, args);

            m_errors.Add(new CompilationError(errorInfo, errorPosition, errorMessage));
        }

        public CompilationError this[int index]
        {
            get { return m_errors[index]; }
        }

        public int Count
        {
            get { return m_errors.Count; }
        }

        public IEnumerator<CompilationError> GetEnumerator()
        {
            foreach (var item in m_errors)
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public CompilationErrorList(CompilationErrorManager errorManager)
        {
            CodeContract.RequiresArgumentNotNull(errorManager, "errorManager");
            m_errors = new List<CompilationError>();
            m_errorManager = errorManager;
        }
    }
}
