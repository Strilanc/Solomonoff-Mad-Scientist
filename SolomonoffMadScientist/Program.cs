using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SolomonoffMadScientist {
    class Program {
        static void Main() {
            var d = new Dictionary<TuringMachine.InstructionSelector, TuringMachine.InstructionResult> {
                {new TuringMachine.InstructionSelector(0, false), new TuringMachine.InstructionResult(0, true, true)},
                {new TuringMachine.InstructionSelector(0, true), new TuringMachine.InstructionResult(-1, true, false)}
            }.ToImmutableDictionary();
            var t = new TuringMachine(d, 2*2*2*2);
            var h = t.AdvancedUntilHalted();
            var n = h.TryGetResult();

            var rx = TuringMachine.PossibleCompleteTuringMachineInstructionsSets().Take(10).ToArray()
                .Select(e => string.Join("; ", e)).ToArray();
        }
    }
}
