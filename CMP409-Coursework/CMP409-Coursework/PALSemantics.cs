using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AllanMilne.Ardkit;

namespace CMP409_Coursework
{
    public class PALSemantics : Semantics
    {
        public PALSemantics(IParser parser)
        : base(parser)
        { }

        // Declare an Identifier
        public void DeclareId(IToken id, int varType)
        {
            if (!id.Is(Token.IdentifierToken)) return;
            Scope symbols = Scope.CurrentScope;
            if (symbols.IsDefined(id.TokenValue))
            {
                semanticError(new AlreadyDeclaredError(id, symbols.Get(id.TokenValue)));
            }
            else
            {
                symbols.Add(new VarSymbol(id, varType));
            }
        }

        // Check the usage of an identifier
        // If it's declared in the current scope
        public int CheckId(IToken id)
        {
            if (!id.Is(Token.IdentifierToken)) return LanguageType.Undefined;

            // Check if it's defined in the current scope
            if (!Scope.CurrentScope.IsDefined(id.TokenValue))
            {
                semanticError(new NotDeclaredError(id));
                return LanguageType.Undefined;
            }
            else
            {
                //return Scope.CurrentScope.Get(id.TokenValue).Type;
                return CheckType(id);
            }
        }

        // Check type compatibility of current token
        public int CheckType(IToken token)
        {
            int thisType = LanguageType.Undefined;
            if (token.Is(Token.IdentifierToken))
            {
                thisType = Scope.CurrentScope.Get(token.TokenValue).Type;
            }
            else if (token.Is(Token.IntegerToken))
            {
                thisType = LanguageType.Integer;
            }
            else if (token.Is(Token.RealToken))
            {
                thisType = LanguageType.Real;
            }

            // If not already set then set the current type being processed
            if (currentType == LanguageType.Undefined)
            {
                currentType = thisType;
            }

            // Found type must be the same as the expected type
            if (currentType != thisType)
            {
               semanticError(new TypeConflictError(token, thisType, currentType));
            }

            return thisType;
        }

        public bool CheckTypesSame(IToken id, int oldType, int newType)
        {
            if (oldType != newType)
            {
                semanticError(new TypeConflictError(id, newType, oldType));
                return false;
            }
            return true;
        }
    }
}
