using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.Compilers
{
    public class CompilationErrorManager
    {
        private CompilationErrorInfoCollection m_errorInfoStore;
        private List<CompilationError> m_errors;
        private ReadOnlyCollection<CompilationError> m_readOnlyErrors;

        public CompilationErrorManager()
        {
            m_errorInfoStore = new CompilationErrorInfoCollection();
            m_errors = new List<CompilationError>();
            m_readOnlyErrors = m_errors.AsReadOnly();
        }

        public ReadOnlyCollection<CompilationError> Errors
        {
            get
            {
                return m_readOnlyErrors;
            }
        }

        public void DefineError(int id, int level, CompilationStage stage, string messageTemplate)
        {
            CodeContract.RequiresArgumentInRange(!m_errorInfoStore.Contains(id), "id", "Error id is duplicated");

            var errorInfo = new CompilationErrorInfo(id, level, stage, messageTemplate);
            m_errorInfoStore.Add(errorInfo);
        }

        public CompilationErrorInfo GetErrorInfo(int id)
        {
            return m_errorInfoStore[id];
        }

        public void AddError(int id, SourceSpan errorPosition, params object[] args)
        {
            CodeContract.RequiresArgumentInRange(m_errorInfoStore.Contains(id), "id", "Error id is invalid");

            var errorInfo = m_errorInfoStore[id];
            var errorMessage = String.Format(errorInfo.MessageTemplate, args);

            m_errors.Add(new CompilationError(errorInfo, errorPosition, errorMessage));
        }

        public void ClearErrors()
        {
            m_errors.Clear();
        }
    }
}
