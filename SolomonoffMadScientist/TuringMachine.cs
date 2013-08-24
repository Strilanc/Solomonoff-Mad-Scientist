using System;
using System.Collections.Immutable;
using System.Numerics;

namespace SolomonoffMadScientist {
    public sealed class TuringMachine {
        public struct InstructionSelector {
            public readonly BigInteger CurrentMachineState;
            public readonly bool CurrentTapeValue;
            public InstructionSelector(BigInteger currentMachineState,
                                       bool currentTapeValue) {
                CurrentMachineState = currentMachineState;
                CurrentTapeValue = currentTapeValue;
            }
        }

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
        }

        public readonly BigInteger Position;
        public readonly BigInteger State;
        public readonly ImmutableHashSet<BigInteger> Tape;
        public readonly ImmutableDictionary<InstructionSelector, InstructionResult> Instructions;
        public bool IsHalted { get { return State < 0; } }

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
