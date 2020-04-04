using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Refactor;
using ABLParser.RCodeReader.Elements;
using System.Collections.Generic;
using System.IO;

namespace ABLParser.Prorefactor.Proparser
{
	/// <summary>
	/// Symbol scope associated with the compilation unit (class or main block of a procedure). It never has a super scope,
	/// but instead TypeInfo object in order to get info from rcode.
	/// </summary>
	public class RootSymbolScope : SymbolScope
	{
		private ITypeInfo typeInfo;

		private readonly ISet<string> functionSet = new SortedSet<string>();

		public RootSymbolScope(RefactorSession session) : base(session)
		{
		}

		public virtual void AttachTypeInfo(ITypeInfo typeInfo)
		{
			this.typeInfo = typeInfo;
		}

		internal virtual void defFunc(string name)
		{
			functionSet.Add(name.ToLower());
		}

		internal override bool IsVariable(string name)
		{
			// First look through the standard way
			if (base.IsVariable(name))
			{
				return true;
			}

			// Then look through rcode
			ITypeInfo info = typeInfo;
			while (info != null)
			{
				if (info.HasProperty(name))
				{
					return true;
				}
				info = Session.GetTypeInfo(info.ParentTypeName);
			}

			return false;
		}

		internal override FieldType IsTableDef(string inName)
		{
			// First look through the standard way
			FieldType ft = base.IsTableDef(inName);
			if (ft != null)
			{
				return ft;
			}

			// Then look through rcode
			ITypeInfo info = typeInfo;
			while (info != null)
			{
				if (info.HasBuffer(inName))
				{
					return FieldType.TTABLE;
				}
				info = Session.GetTypeInfo(info.ParentTypeName);
			}

			return null;
		}

		/// <summary>
		/// methodOrFunc should only be called for the "unit" scope, since it is the only one that would ever contain methods
		/// or user functions.
		/// </summary>
		internal override int IsMethodOrFunction(string name)
		{
			string lname = name.ToLower();
			// Methods take precedent over built-in functions. The compiler (10.2b)
			// does not seem to try recognize by function/method signature.
			ITypeInfo info = typeInfo;
			while (info != null)
			{
				if (info.HasMethod(name))
				{
					return ABLNodeType.LOCAL_METHOD_REF.Type;
				}
				info = Session.GetTypeInfo(info.ParentTypeName);
			}

			if (functionSet.Contains(lname))
			{
				return ABLNodeType.USER_FUNC.Type;
			}

			return 0;
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void writeScope(Writer writer) throws IOException
		public override void WriteScope(StreamWriter writer)
		{
			writer.Write("*** RootSymbolScope *** \n");
			base.WriteScope(writer);
			foreach(string e in functionSet)
			{
				try
				{
					writer.Write("Function " + e + "\n");
				}
				catch (IOException)
				{
				}
			};
		}

	}

}
