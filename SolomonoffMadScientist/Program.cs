using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Strilanc.Value;

internal class Program {
    private static void Main() {
        var d = new Dictionary<TuringMachine.InstructionSelector, TuringMachine.InstructionResult> {
            {new TuringMachine.InstructionSelector(0, false), new TuringMachine.InstructionResult(0, true, true)},
            {new TuringMachine.InstructionSelector(0, true), new TuringMachine.InstructionResult(-1, true, false)}
        }.ToImmutableDictionary();
        var t = new TuringMachine(d, 2*2*2*2);
        var h = t.AdvancedUntilHalted();
        var n = h.MayResult;

        var zz = new List<string[]>();

        var s = new SolomonoffInductor();
        for (var i = 0; i < 25; i++) {
            s.Advance(10000);
            var z = s.Predict()
                .OrderBy(e => e.Key.Else(-1))
                .Select(e => string.Format("{0}: {1:0.000000}", e.Key, (double)e.Value))
                .ToArray();
            zz.Add(z);
            s.Measure(1, nextInput: i+1);
        }
    }
}
