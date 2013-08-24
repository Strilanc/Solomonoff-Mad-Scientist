using System;
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

        var s = new SolomonoffInductor();
        for (var i = 0; ; i++) {
            s.Advance(i*i+10);
            var z = s.Predict()
                .OrderBy(e => -e.Value)
                .Select(e => string.Format("{0}: {1:0.000000}", e.Key, (double)e.Value))
                .ToArray();
            Console.WriteLine("=== Step: " + i);
            foreach (var s1 in z) {
                Console.WriteLine(s1);
            }
            s.Measure(1, nextInput: i+1);
        }
    }
}
