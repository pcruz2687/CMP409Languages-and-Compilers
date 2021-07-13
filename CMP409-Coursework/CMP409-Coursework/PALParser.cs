using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using AllanMilne.Ardkit;

namespace CMP409_Coursework
{
    public class PALParser : RecoveringRdParser
    {
        private PALSemantics semantics;

        public PALParser()
        : base (new PALScanner ())
        {
            semantics = new PALSemantics(this);
        }

        // recStarter()
        protected override void recStarter()
        {
            Scope.OpenScope();
            if (have("PROGRAM"))
            {
                mustBe("PROGRAM");
                mustBe(Token.IdentifierToken);
                mustBe("WITH");
                recVarDecls();
                mustBe("IN");

                do {
                    recStatement();
                } while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"));

                mustBe("END");
            }
            Scope.CloseScope();
        }

        // recVarDecls()
        private void recVarDecls()
        {
            int varType;

            List<IToken> identList = new List<IToken>();
            while(have(Token.IdentifierToken))
            {
                identList = recIdentList();
                mustBe("AS");
                varType = recType();

                if(varType != 0)
                {
                    foreach (IToken ident in identList)
                    {
                        semantics.DeclareId(ident, varType);
                    }
                }
            }
        }
        
        // recIdentList()
        private List<IToken> recIdentList()
        {
            List<IToken> identList = new List<IToken>();
            identList.Add(scanner.CurrentToken);
            mustBe(Token.IdentifierToken);

            while(have(","))
            {
                mustBe(",");
                identList.Add(scanner.CurrentToken);
                mustBe(Token.IdentifierToken);
            }

            return identList;
        }

        // recType()
        private int recType()
        {
            int varType = 0;
            if(have("REAL"))
            {
                mustBe("REAL");
                varType = LanguageType.Real;
            }
            else if(have("INTEGER"))
            {
                mustBe("INTEGER");
                varType = LanguageType.Integer;
            }
            else
            {
                syntaxError("REAL or INTEGER");
                varType = LanguageType.Undefined;
            }

            return varType;
        }

        // recStatement()
        private void recStatement()
        {
            semantics.ResetCurrentType();
            if(have(Token.IdentifierToken))
            {
                recAssignment();
            }
            else if(have("UNTIL"))
            {
                recLoop();
            }
            else if (have("IF"))
            {
                recConditional();
            }
            else if(have("INPUT") || have("OUTPUT"))
            {
                recIO();
            }
            else
            {
                syntaxError("<Statement>");
            }
        }

        // recAssignment()
        private void recAssignment()
        {
            int varType = semantics.CheckId(scanner.CurrentToken);
            mustBe(Token.IdentifierToken);
            mustBe("=");
            IToken assignToken = scanner.CurrentToken;
            int exprType = recExpression();
        }

        // recExpression()
        private int recExpression()
        {
            int termType = recTerm();

            while(have("+") || have("-"))
            {
                if(have("+"))
                {
                    mustBe("+");
                }
                else if(have("-"))
                {
                    mustBe("-");
                }

                IToken exprToken = scanner.CurrentToken;
                int termType2 = recTerm();
                semantics.CheckTypesSame(exprToken, termType, termType2);
            }

            return termType;
        }

        // recTerm()
        private int recTerm()
        {
            IToken termToken1 = scanner.CurrentToken;
            int factType = recFactor();

            while (have("*") || have("/"))
            {
                if (have("*"))
                {
                    mustBe("*");
                }
                else if (have("/"))
                {
                    mustBe("/");
                }

                IToken termToken = scanner.CurrentToken;
                int factType2 = recFactor();


                if (factType != 0)
                {
                    semantics.CheckTypesSame(termToken, factType, factType2);
                }
            }

            return factType;
        }

        // recFactor()
        private int recFactor()
        {
            if(have("+")) {
                mustBe("+");
            }
            else if (have("-"))
            {
                mustBe("-");
            }

            if(have(Token.IdentifierToken) || have(Token.RealToken) || have(Token.IntegerToken))
            {
                int valType = recValue();
                return valType;
            }
            else if (have("("))
            {
                mustBe("(");
                int exprType = recExpression();
                mustBe(")");
                return exprType;
            } 
            else
            {
                syntaxError("Value or Expression");
                return semantics.CheckType(scanner.CurrentToken);
            }
        }

        // recValue()
        private int recValue()
        {
            int valType;

            if (have(Token.IdentifierToken))
            {
                valType = semantics.CheckId(scanner.CurrentToken);
                mustBe(Token.IdentifierToken);
            }
            else if (have(Token.IntegerToken))
            {
                valType = semantics.CheckType(scanner.CurrentToken);
                mustBe(Token.IntegerToken);
            }
            else if (have(Token.RealToken))
            {
                valType = semantics.CheckType(scanner.CurrentToken);
                mustBe(Token.RealToken);
            }
            else
            {
                valType = semantics.CheckType(scanner.CurrentToken);
            }

            return valType;
        }

        // recLoop()
        private void recLoop()
        {
            mustBe("UNTIL");
            bool booleanExpr = recBooleanExpr();
            mustBe("REPEAT");

            if(booleanExpr)
            {
                while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"))
                {
                    recStatement();
                }
            }
            

            mustBe("ENDLOOP");
        }

        // recBooleanExpr()
        private bool recBooleanExpr()
        {
            int exprType = recExpression();
            if(have("<"))
            {
                mustBe("<");
            }
            else if(have("="))
            {
                mustBe("=");
            }
            else if(have(">"))
            {
                mustBe(">");
            }

            IToken boolToken = scanner.CurrentToken;
            int exprType2 = recExpression();

            bool booleanExprType = false;

            if (exprType != 0)
            {
                booleanExprType = semantics.CheckTypesSame(boolToken, exprType, exprType2);
            }

            return booleanExprType;
        }

        // recConditional()
        private void recConditional()
        {
            mustBe("IF");
            bool booleanExpr = recBooleanExpr();
            mustBe("THEN");
            
            if(booleanExpr)
            {
                while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"))
                {
                    recStatement();
                }
            }
  
            if (have("ELSE"))
            {
                mustBe("ELSE");
                while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"))
                {
                    recStatement();
                }
            }

            mustBe("ENDIF");
        }

        // recIO()
        private void recIO()
        {
            if(have("INPUT"))
            {
                mustBe("INPUT");

                List<IToken> identList = new List<IToken>();
                identList = recIdentList();
                
                identList.ForEach((ident) =>
                {
                    semantics.ResetCurrentType();
                    semantics.CheckId(ident);
                });

            }
            else if(have("OUTPUT"))
            {
                mustBe("OUTPUT");
                recExpression();

                while(have(","))
                {
                    semantics.ResetCurrentType();
                    mustBe(",");
                    recExpression();
                }
            }
        }
    }
}