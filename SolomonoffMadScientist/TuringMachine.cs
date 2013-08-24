using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace SolomonoffMadScientist {
    public sealed class TuringMachine {
        [DebuggerDisplay("{ToString()}")]
        public struct InstructionSelector {
            public readonly BigInteger CurrentMachineState;
            public readonly bool CurrentTapeValue;
            public InstructionSelector(BigInteger currentMachineState,
                                       bool currentTapeValue) {
                CurrentMachineState = currentMachineState;
                CurrentTapeValue = currentTapeValue;
            }
            public override string ToString() {
                return string.Format("CurState={0},CurTape={1}",
                                     CurrentMachineState,
                                     CurrentTapeValue);
            }
        }

        [DebuggerDisplay("{ToString()}")]
        public struct InstructionResult {
            public readonly BigInteger NewMachineState;
            public readonly bool NewTapeValue;
            public readonly bool ThenMoveRightward;
            public InstructionResult(BigInteger newMachineState,
                                     bool newTapeValue,
                                     bool thenMoveRightward) {
                NewMachineState = newMachineState;
                NewTapeValue = newTapeValue;
                ThenMoveRightward = thenMoveRightward;
            }
            public bool ThenMoveLeftward { get { return !ThenMoveRightward; } }
            public override string ToString() {
                return string.Format("NewState={0},NewTape={1},Move={2}",
                                     NewMachineState,
                                     NewTapeValue,
                                     ThenMoveRightward ? "Right" : "Left");
            }
        }

        public readonly BigInteger Position;
        public readonly BigInteger State;
        public readonly ImmutableHashSet<BigInteger> Tape;
        public readonly ImmutableDictionary<InstructionSelector, InstructionResult> Instructions;
        public bool IsHalted { get { return !Instructions.ContainsKey(new InstructionSelector(State, Tape.Contains(Position))); } }

        public static IEnumerable<ImmutableDictionary<InstructionSelector, InstructionResult>> PossibleCompleteTuringMachineInstructionsSets() {
            var haltState = new InstructionResult(-1, false, false);
            return from stateCount in CollectionUtil.Naturals()
                   let inputSpace = stateCount.Range()
                                              .Cross(new[] {false, true})
                                              .Select(e => new InstructionSelector(e.Item1, e.Item2))
                   let outputSpace = stateCount.Range()
                                               .Cross(new[] {false, true})
                                               .Cross(new[] {false, true})
                                               .Select(e => new InstructionResult(
                                                                e.Item1.Item1,
                                                                e.Item1.Item2,
                                                                e.Item2))
                                               .Prepend(haltState)
                   let inputsToOutputs = from input in inputSpace
                                         select from output in outputSpace
                                                select new { input, output }
                   from mapping in inputsToOutputs.AllChoiceCombinations() 
                   select mapping.ToImmutableDictionary(e => e.input, e => e.output);
        }

        public TuringMachine(ImmutableDictionary<InstructionSelector, InstructionResult> instructions, BigInteger input) {
            if (instructions == null) throw new ArgumentNullException("instructions");
            Instructions = instructions;
            Position = 0;
            State = 0;
            Tape = ImmutableHashSet.Create<BigInteger>();

            for (var i = 0; input > 0; i++) {
                if (!input.IsEven) Tape = Tape.Add(i);
                input >>= 1;
            }
        }
        public TuringMachine(ImmutableDictionary<InstructionSelector, InstructionResult> instructions, BigInteger position, ImmutableHashSet<BigInteger> tape, BigInteger state) {
            if (instructions == null) throw new ArgumentNullException("instructions");
            if (tape == null) throw new ArgumentNullException("tape");
            Instructions = instructions;
            Position = position;
            Tape = tape;
            State = state;
        }

        public BigInteger? TryGetResult() {
            if (!IsHalted) return null;
            var result = BigInteger.Zero;
            for (var i = Position - 1; i >= 0; i--) {
                result *= 2;
                if (Tape.Contains(i)) result += 1;
            }
            return result;
        }
        public TuringMachine AdvancedUntilHalted() {
            var t = this;
            while (true) {
                var n = t.Advanced();
                if (n == t) return t;
                t = n;
            }
        }
        public TuringMachine Advanced() {
            if (IsHalted) return this;

            var instruction = Instructions[new InstructionSelector(State, Tape.Contains(Position))];
            var newTape = instruction.NewTapeValue ? Tape.Add(Position) : Tape.Remove(Position);
            var newState = instruction.NewMachineState;
            var newPosition = Position + (instruction.ThenMoveRightward ? 1 : -1);
            return new TuringMachine(Instructions, newPosition, newTape, newState);
        }
    }
}
