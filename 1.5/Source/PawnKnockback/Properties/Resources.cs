using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace PawnKnockback.Properties
{
	// Token: 0x0200000B RID: 11
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x06000032 RID: 50 RVA: 0x00003EAD File Offset: 0x000020AD
		internal Resources()
		{
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000033 RID: 51 RVA: 0x00003EB8 File Offset: 0x000020B8
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				bool flag = Resources.resourceMan == null;
				if (flag)
				{
					Resources.resourceMan = new ResourceManager("PawnKnockback.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000034 RID: 52 RVA: 0x00003EFC File Offset: 0x000020FC
		// (set) Token: 0x06000035 RID: 53 RVA: 0x00003F13 File Offset: 0x00002113
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x04000044 RID: 68
		private static ResourceManager resourceMan;

		// Token: 0x04000045 RID: 69
		private static CultureInfo resourceCulture;
	}
}
