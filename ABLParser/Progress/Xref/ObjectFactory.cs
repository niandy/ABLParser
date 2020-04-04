
namespace ABLParser.Progress.Xref
{
	/// <summary>
	/// This object contains factory methods for each 
	/// Java content interface and Java element interface 
	/// generated in the uri.schemas_progress_com.xrefd._0005 package. 
	/// <para>An ObjectFactory allows you to programatically 
	/// construct new instances of the Java representation 
	/// for XML content. The Java representation of XML 
	/// content can consist of schema derived interfaces 
	/// and classes representing the binding of schema 
	/// type definitions, element declarations and model 
	/// groups.  Factory methods for each of these are 
	/// provided in this class.
	/// 
	/// </para>
	/// </summary>
	//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
	//ORIGINAL LINE: @XmlRegistry public class ObjectFactory
	public class ObjectFactory
	{
		/// <summary>
		/// Create a new ObjectFactory that can be used to create new instances of schema derived classes for package: uri.schemas_progress_com.xrefd._0005
		/// 
		/// </summary>
		public ObjectFactory()
		{
		}

		/// <summary>
		/// Create an instance of <seealso cref="CrossReference "/>
		/// 
		/// </summary>
		public virtual Crossreference CreateCrossReference()
		{
			return new Crossreference();
		}

		/// <summary>
		/// Create an instance of <seealso cref="CrossReference.Source "/>
		/// 
		/// </summary>
		public virtual CrossreferenceSource CreateCrossReferenceSource()
		{
			return new CrossreferenceSource();
		}

		/// <summary>
		/// Create an instance of <seealso cref="CrossReference.Source.Reference "/>
		/// 
		/// </summary>
		public virtual CrossreferenceSourceReference CreateCrossReferenceSourceReference()
		{
			return new CrossreferenceSourceReference();
		}

		/// <summary>
		/// Create an instance of <seealso cref="CrossReference.Source.Reference.DatasetRef "/>
		/// 
		/// </summary>
		public virtual CrossreferenceSourceReferenceDatasetref CreateCrossReferenceSourceReferenceDatasetRef()
		{
			return new CrossreferenceSourceReferenceDatasetref();
		}

		/// <summary>
		/// Create an instance of <seealso cref="CrossReference.Source.Reference.ClassRef "/>
		/// 
		/// </summary>
		public virtual CrossreferenceSourceReferenceClassref CreateCrossReferenceSourceReferenceClassRef()
		{
			return new CrossreferenceSourceReferenceClassref();
		}

		/// <summary>
		/// Create an instance of <seealso cref="CrossReference.Source.Reference.StringRef "/>
		/// 
		/// </summary>
		public virtual CrossreferenceSourceReferenceStringref CreateCrossReferenceSourceReferenceStringRef()
		{
			return new CrossreferenceSourceReferenceStringref();
		}

		/// <summary>
		/// Create an instance of <seealso cref="CrossReference.Source.Reference.ParameterRef "/>
		/// 
		/// </summary>
		public virtual CrossreferenceSourceReferenceParameterref CreateCrossReferenceSourceReferenceParameterRef()
		{
			return new CrossreferenceSourceReferenceParameterref();
		}

		/// <summary>
		/// Create an instance of <seealso cref="CrossReference.Source.Reference.InterfaceRef "/>
		/// 
		/// </summary>
		public virtual CrossreferenceSourceReferenceInterfaceref CreateCrossReferenceSourceReferenceInterfaceRef()
		{
			return new CrossreferenceSourceReferenceInterfaceref();
		}

		/// <summary>
		/// Create an instance of <seealso cref="CrossReference.Source.Reference.DatasetRef.Relation "/>
		/// 
		/// </summary>
		public virtual CrossreferenceSourceReferenceDatasetrefRelation CreateCrossReferenceSourceReferenceDatasetRefRelation()
		{
			return new CrossreferenceSourceReferenceDatasetrefRelation();
		}

	}

}
