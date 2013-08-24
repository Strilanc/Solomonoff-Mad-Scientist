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
    public readonly Tuple<BigInteger, ImmutableHashSet<BigInteger>>  PreviousState;
    public bool IsHalted { get { return Position < 0 || !Instructions.ContainsKey(new InstructionSelector(State, Tape.Contains(Position))); } }
    public bool IsStuckInALoop { get { return Equals(PreviousState, Tuple.Create(State, Tape)); } }

    public TuringMachine(ImmutableDictionary<InstructionSelector, InstructionResult> instructions, BigInteger input) {
        if (instructions == null) throw new ArgumentNullException("instructions");
        Instructions = instructions;
        Position = 0;
        State = 0;
        ElapsedSteps = 0;
        Tape = ImmutableHashSet.Create<BigInteger>();
        PreviousState = Tuple.Create(BigInteger.Zero, new[] { -BigInteger.One }.ToImmutableHashSet());

        for (var i = 0; input > 0; i++) {
            Tape = Tape.Add(i*2 + (input.IsEven ? 0 : 1));
            input >>= 1;
        }
    }
    public TuringMachine(ImmutableDictionary<InstructionSelector, InstructionResult> instructions,
                         BigInteger position,
                         ImmutableHashSet<BigInteger> tape,
                         BigInteger state,
                         BigInteger elapsedSteps,
                         Tuple<BigInteger, ImmutableHashSet<BigInteger>> previousState) {
        if (instructions == null) throw new ArgumentNullException("instructions");
        if (tape == null) throw new ArgumentNullException("tape");
        Instructions = instructions;
        Position = position;
        Tape = tape;
        State = state;
        ElapsedSteps = elapsedSteps;
        PreviousState = previousState;
    }

    public May<BigInteger> MayResult {
        get {
            if (!IsHalted) return May.NoValue;
            var result = BigInteger.Zero;
            for (var i = Position; i >= 0; i--) {
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
    public TuringMachine Advanced(int steps) {
        var cur = this;
        for (var i = 0; i < steps; i++) {
            cur = cur.Advanced();
        }
        return cur;
    }
    public TuringMachine Advanced() {
        if (IsHalted) return this;
        if (IsStuckInALoop) return this;

        var curState = new InstructionSelector(State, Tape.Contains(Position));
        var instruction = Instructions[curState];
        var newState = instruction.NewMachineState;
        var newTape = instruction.NewTapeValue ? Tape.Add(Position) : Tape.Remove(Position);
        var newPosition = Position + (newState < 0 ? 0 : instruction.ThenMoveRightward ? 1 : -1);
        var newPreviousState = ElapsedSteps.IsPowerOfTwo ? Tuple.Create(State, Tape) : PreviousState;
        return new TuringMachine(Instructions, newPosition, newTape, newState, ElapsedSteps + 1, newPreviousState);
    }
}
