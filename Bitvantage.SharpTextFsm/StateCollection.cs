using System.Collections;
using System.Text;

namespace Bitvantage.SharpTextFsm
{
    public class StateCollection : IReadOnlyList<TemplateState>
    {
        public Template Template { get; }

        private readonly LookupList<string, TemplateState> _states = new(item=>item.Name);
        public IEnumerator<TemplateState> GetEnumerator()
        {
            return _states.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_states).GetEnumerator();
        }

        public int Count => _states.Count;

        internal void Add(TemplateState state)
        {
            if (state.Name == "~Global")
                GlobalState = state;

            _states.Add(state);
        }

        public TemplateState this[int index] => _states[index];

        public TemplateState this[string key] => _states[key];

        public TemplateState? GlobalState { get; set; }

        internal StateCollection(Template template)
        {
            Template = template;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var state in this)
            {
                sb.AppendLine($"{state.Name}");

                foreach (var rule in state.Rules)
                    sb.AppendLine($"{rule.ToString()}");

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
