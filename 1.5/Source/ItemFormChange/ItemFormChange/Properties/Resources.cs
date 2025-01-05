using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace YR_ItemFormChange.Properties
{
    // Token: 0x02000008 RID: 8
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [DebuggerNonUserCode]
    [CompilerGenerated]
    internal class Resources
    {
        // Token: 0x0600001A RID: 26 RVA: 0x00002854 File Offset: 0x00000A54
        internal Resources()
        {
        }

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x0600001B RID: 27 RVA: 0x00002860 File Offset: 0x00000A60
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                bool flag = Resources.resourceMan == null;
                if (flag)
                {
                    ResourceManager resourceManager = new ResourceManager("YR_ItemFormChange.Properties.Resources", typeof(Resources).Assembly);
                    Resources.resourceMan = resourceManager;
                }
                return Resources.resourceMan;
            }
        }

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x0600001C RID: 28 RVA: 0x000028A8 File Offset: 0x00000AA8
        // (set) Token: 0x0600001D RID: 29 RVA: 0x000028BF File Offset: 0x00000ABF
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

        // Token: 0x04000016 RID: 22
        private static ResourceManager resourceMan;

        // Token: 0x04000017 RID: 23
        private static CultureInfo resourceCulture;
    }
}
