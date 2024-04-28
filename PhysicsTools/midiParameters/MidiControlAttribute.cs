using System;

namespace MyUtilities.Physics.MidiParameters
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MidiControlAttribute : Attribute
    {
        public string ControlName { get; }

        public MidiControlAttribute(string controlName)
        {
            ControlName = controlName;
        }
    }
}


