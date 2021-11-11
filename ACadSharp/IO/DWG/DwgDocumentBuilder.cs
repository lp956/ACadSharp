﻿using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgDocumentBuilder
	{
		public DwgHeaderHandlesCollection HeaderHandles { get; set; }

		public DwgBlockCtrlObjectTemplate BlockControlTemplate { get; set; }

		public List<DwgBlockTemplate> BlockHeaders { get; } = new List<DwgBlockTemplate>();
		public List<ICadObjectBuilder> Blocks { get; } = new List<ICadObjectBuilder>();

		public DwgTableTemplate<AppId> AppIds { get; set; }

		public Dictionary<ulong, DwgTemplate> Templates { get; } = new Dictionary<ulong, DwgTemplate>();
		public Dictionary<ulong, CadObject> ObjectsMap { get; } = new Dictionary<ulong, CadObject>();

		public CadDocument DocumentToBuild { get; }

		public DwgDocumentBuilder(CadDocument document)
		{
			this.DocumentToBuild = document;
		}

		public void BuildDocument()
		{
			if (this.AppIds != null)
				this.AppIds.Build(this);

			if (this.HeaderHandles.BYLAYER != null && this.TryGetCadObject(this.HeaderHandles.BYLAYER.Value, out LineType lineType))
				this.DocumentToBuild.LineTypes.Add(lineType);

			if (this.HeaderHandles.BYBLOCK != null && this.TryGetCadObject(this.HeaderHandles.BYBLOCK.Value, out LineType byBlock))
				this.DocumentToBuild.LineTypes.Add(byBlock);

			if (this.HeaderHandles.CONTINUOUS != null && this.TryGetCadObject(this.HeaderHandles.CONTINUOUS.Value, out LineType continuous))
				this.DocumentToBuild.LineTypes.Add(continuous);

			foreach (DwgBlockTemplate header in this.BlockHeaders)
				header.Build(this);

			foreach (ICadObjectBuilder block in this.Blocks)
				block.Build(this);

			if (this.BlockControlTemplate != null)
				this.BlockControlTemplate.Build(this);

			//foreach (ICadObjectBuilder item in this.DictionaryBuilders)
			//	item.Build(this);

			//foreach (ICadObjectBuilder item in this.DictionaryChildBuilders)
			//	item.Build(this);

			if (this.HeaderHandles.DIMSTYLE != null && this.TryGetCadObject(this.HeaderHandles.DIMSTYLE.Value, out DimensionStyle dstyle))
				this.DocumentToBuild.DimensionStyles.Add(dstyle);

			//Build all the objects in the document
			//foreach (ICadObjectBuilder item in Templates.Values.Where(b => b.ToBuild))
			//	item.Build(this);

			//foreach (var obj in this.ViewportTemplates)
			//	obj.Build(this);

		}

		public CadObject GetCadObject(ulong handle)
		{
			if (this.ObjectsMap.TryGetValue(handle, out CadObject cadObject))
				return cadObject;
			else
				return null;
		}

		public T GetCadObject<T>(ulong handle) where T : CadObject
		{
			if (this.ObjectsMap.TryGetValue(handle, out CadObject cadObject))
			{
				if (cadObject is T)
					return (T)cadObject;
			}

			return null;
		}

		public bool TryGetCadObject<T>(ulong handle, out T value) where T : CadObject
		{
			if (this.ObjectsMap.TryGetValue(handle, out CadObject cadObject))
			{
				if (cadObject is T)
				{
					value = (T)cadObject;
					return true;
				}
			}

			value = null;
			return false;
		}

		public T GetObjectBuilder<T>(ulong handle) where T : DwgTemplate
		{
			if (this.Templates.TryGetValue(handle, out DwgTemplate builder))
			{
				return (T)builder;
			}

			return null;
		}

		public bool TryGetObjectBuilder<T>(ulong handle, out T value) where T : DwgTemplate
		{
			if (this.Templates.TryGetValue(handle, out DwgTemplate template))
			{
				if (template is T)
				{
					value = (T)template;
					return true;
				}
			}

			value = null;
			return false;
		}

		[Obsolete]
		public void BuildObjects()
		{
			foreach (var item in this.Templates.Values)
			{
				item.Build(this);
			}
		}
	}
}