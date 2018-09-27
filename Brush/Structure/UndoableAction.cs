using System;

namespace Brush.Structure
{
    public class UndoableAction
    {
        public string Name { get; set; }
        public Action Do;
        public Action Undo;

        public override string ToString()
        {
            return Name;
        }
    }
}