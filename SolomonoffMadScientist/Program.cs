using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

internal class Program {
    private static void Main() {
        var d = new Dictionary<TuringMachine.InstructionSelector, TuringMachine.InstructionResult> {
            {new TuringMachine.InstructionSelector(0, false), new TuringMachine.InstructionResult(0, true, true)},
            {new TuringMachine.InstructionSelector(0, true), new TuringMachine.InstructionResult(-1, true, false)}
        }.ToImmutableDictionary();
        var t = new TuringMachine(d, 2*2*2*2);
        var h = t.AdvancedUntilHalted();
        var n = h.MayResult;

        var ss = SolomonoffInductor.PossibleCompleteTuringMachineInstructionsSets().Take(100).ToArray();

        var s = new SolomonoffInductor();
        for (var i = 0; i < 100000; i++) {
            s.Advance();
        }
        var z = s.Predict()
            .Select(e => string.Format("{0}: {1:0.000000}", e.Key, (double)e.Value))
            .ToArray();
        
        s.Measure(1, nextInput: 1);
        for (var i = 0; i < 100000; i++) {
            s.Advance();
        }
        var z2 = s.Predict()
            .Select(e => string.Format("{0}: {1:0.000000}", e.Key, (double)e.Value))
            .ToArray();

        s.Measure(1, nextInput: 2);
        for (var i = 0; i < 100000; i++) {
            s.Advance();
        }
        var z3 = s.Predict()
            .Select(e => string.Format("{0}: {1:0.000000}", e.Key, (double)e.Value))
            .ToArray();
    }
}
