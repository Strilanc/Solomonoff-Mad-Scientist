using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using Strilanc.Value;

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
    public readonly BigInteger ElapsedSteps;
    public readonly BigInteger State;
    public readonly ImmutableHashSet<BigInteger> Tape;
    public readonly ImmutableDictionary<InstructionSelector, InstructionResult> Instructions;
    public bool IsHalted { get { return Position < 0 || !Instructions.ContainsKey(new InstructionSelector(State, Tape.Contains(Position))); } }

    public TuringMachine(ImmutableDictionary<InstructionSelector, InstructionResult> instructions, BigInteger input) {
        if (instructions == null) throw new ArgumentNullException("instructions");
        Instructions = instructions;
        Position = 0;
        State = 0;
        ElapsedSteps = 0;
        Tape = ImmutableHashSet.Create<BigInteger>();

        for (var i = 0; input > 0; i++) {
            if (!input.IsEven) Tape = Tape.Add(i);
            input >>= 1;
        }
    }
    public TuringMachine(ImmutableDictionary<InstructionSelector, InstructionResult> instructions,
                         BigInteger position,
                         ImmutableHashSet<BigInteger> tape,
                         BigInteger state,
                         BigInteger elapsedSteps) {
        if (instructions == null) throw new ArgumentNullException("instructions");
        if (tape == null) throw new ArgumentNullException("tape");
        Instructions = instructions;
        Position = position;
        Tape = tape;
        State = state;
        ElapsedSteps = elapsedSteps;
    }

    public May<BigInteger> MayResult {
        get {
            if (!IsHalted) return May.NoValue;
            var result = BigInteger.Zero;
            for (var i = Position - 1; i >= 0; i--) {
                result *= 2;
                if (Tape.Contains(i)) result += 1;
            }
            return result;
        }
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
        var newState = instruction.NewMachineState;
        if (newState < 0) {
            // explicit halt
            return new TuringMachine(Instructions, Position, Tape, newState, ElapsedSteps + 1);
        }
        var newTape = instruction.NewTapeValue ? Tape.Add(Position) : Tape.Remove(Position);
        var newPosition = Position + (instruction.ThenMoveRightward ? 1 : -1);
        return new TuringMachine(Instructions, newPosition, newTape, newState, ElapsedSteps + 1);
    }
}
