﻿namespace NLox.Interpreter.Statements;

using NLox.Interpreter.Expressions;

public abstract record Statement(IExpression Expression);