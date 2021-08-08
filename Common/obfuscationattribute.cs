using System;

namespace CryptoObfuscatorHelper
{
    [AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Parameter | AttributeTargets.Interface |
    AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Method | AttributeTargets.Enum | AttributeTargets.Struct |
    AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    [CryptoObfuscatorHelper.Obfuscation]
    public class ObfuscationAttribute : Attribute
    {
        private bool _applyToMembers = true;
        private bool _exclude = true;
        private string _feature = "default";
        private bool _strip = true;

        public bool Exclude
        {
            get { return this._exclude; }
            set { this._exclude = value; }
        }

        public string Feature
        {
            get { return this._feature; }
            set { this._feature = value; }
        }

        public bool ApplyToMembers
        {
            get { return this._applyToMembers; }
            set { this._applyToMembers = value; }
        }

        public bool StripAfterObfuscation
        {
            get { return this._strip; }
            set { this._strip = value; }
        }
    }


    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    class ObfuscationRuleAttribute : Attribute
    {
        public ObfuscationRuleAttribute()
        { }

        private RuleType type = RuleType.DoNotObfuscate;
        public RuleType Type
        {
            get { return type; }
            set { type = value; }
        }

        private RuleTarget target = RuleTarget.None;
        public RuleTarget Target
        {
            get { return target; }
            set { target = value; }
        }

        private RuleVisibility visibility = RuleVisibility.All;
        public RuleVisibility Visibility
        {
            get { return visibility; }
            set { visibility = value; }
        }

        private string pattern = string.Empty;
        public string Pattern
        {
            get { return pattern; }
            set { pattern = value; }
        }

        private string baseClassPattern = string.Empty;
        public string BaseClassPattern
        {
            get { return baseClassPattern; }
            set { baseClassPattern = value; }
        }

        private bool immediateParentOnly = true;
        public bool ImmediateParentOnly
        {
            get { return immediateParentOnly; }
            set { immediateParentOnly = value; }
        }

        private bool _strip = true;
        public bool StripAfterObfuscation
        {
            get { return this._strip; }
            set { this._strip = value; }
        }
        
        private bool _force = false;
        public bool Force
        {
            get { return this._force; }
            set { this._force = value; }
        }
    }

    enum RuleType
    {
        DoNotObfuscate,
        Obfuscate,
    }

    [Flags]
    enum RuleTarget
    {
        None = 0,
        Classes = 1,
        Fields = Classes << 1,
        Properties = Fields << 1,
        Methods = Properties << 1,
        Events = Methods << 1,
        Resources = Events << 1,
        All = Classes | Fields | Properties | Methods | Events | Resources,
    }

    [Flags]
    enum RuleVisibility
    {
        None = 0,
        Public = 1,
        Private = Public << 1,
        Protected = Private << 1,
        Internal = Protected << 1,
        ProtectedInternal = Internal << 1,
        All = Public | Private | Protected | Internal | ProtectedInternal,
    }

}
