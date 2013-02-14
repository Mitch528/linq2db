﻿using System;
using LinqToDB.Data;

namespace LinqToDB.Linq
{
	using DataProvider;
	using SqlProvider;
	using Mapping;

	class DefaultDataContextInfo : IDataContextInfo
	{
		private IDataContext    _dataContext;
		public  IDataContext     DataContext      { get { return _dataContext ?? (_dataContext = new DataConnection()); } }

		public MappingSchemaOld  MappingSchema    { get { return Map.DefaultSchema; } }
		public bool              DisposeContext   { get { return true; } }
		public SqlProviderFlags  SqlProviderFlags { get { return _dataProvider.SqlProviderFlags; } }
		public string            ContextID        { get { return _dataProvider.Name; } }

		public ISqlProvider CreateSqlProvider()
		{
			return _dataProvider.CreateSqlProvider();
		}

		public IDataContextInfo Clone(bool forNestedQuery)
		{
			return new DataContextInfo(DataContext.Clone(forNestedQuery));
		}

		static readonly IDataProvider _dataProvider = DataConnection.GetDataProvider(DataConnection.DefaultConfiguration);
	}
}