using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Numerics;
using Strilanc.Value;

public sealed class SolomonoffInductor {
    public delegate Tuple<Hypothesis, HypothesisGenerator> HypothesisGenerator();
    public struct Hypothesis : IComparable<Hypothesis> {
        public readonly BigRational Prior;
        public readonly TuringMachine State;
        public readonly BigInteger MaxSeenElapsedSteps;
        public Hypothesis(BigRational prior, TuringMachine state, BigInteger maxSeenElapsedSteps) {
            Prior = prior;
            State = state;
            MaxSeenElapsedSteps = maxSeenElapsedSteps;
        }
        public BigRational Weight { get { return Prior / MaxSeenElapsedSteps; } }
        public int CompareTo(Hypothesis other) {
            return other.Weight.CompareTo(Weight);
        }
    }

    public static IEnumerable<ImmutableDictionary<TuringMachine.InstructionSelector, TuringMachine.InstructionResult>> PossibleCompleteTuringMachineInstructionsSets() {
        var haltState = new TuringMachine.InstructionResult(
            newMachineState: -1, 
            newTapeValue: false, 
            thenMoveRightward: false);
        return from stateCount in CollectionUtil.Naturals()
               let inputSpace = stateCount.Range()
                                          .Cross(new[] { false, true })
                                          .Select(e => new TuringMachine.InstructionSelector(e.Item1, e.Item2))
               let outputSpace = stateCount.Range()
                                           .Cross(new[] { false, true })
                                           .Cross(new[] { false, true })
                                           .Select(e => new TuringMachine.InstructionResult(
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

    private readonly AscendingPriorityQueue<Hypothesis> _runningHypotheses = new AscendingPriorityQueue<Hypothesis>();
    private readonly List<Hypothesis> _finishedHypotheses = new List<Hypothesis>();
    private readonly IEnumerator<Hypothesis> _unexploredHypotheses;
    private static readonly BigRational Base = new BigRational(9, 10);

    public SolomonoffInductor() {
        var totalWeight = 1 / (1 - Base);
        _runningHypotheses.Enqueue(new Hypothesis(totalWeight, null, 1));
        _unexploredHypotheses = 
            PossibleCompleteTuringMachineInstructionsSets()
            .Zip(Base.Powers(), (e1, e2) => new Hypothesis(e2, new TuringMachine(e1, 0), 1))
            .GetEnumerator();
    }

    public IEnumerable<KeyValuePair<May<BigInteger>, BigRational>> Predict() {
        var incompleteWeight = _runningHypotheses.Select(e => e.Weight).Sum();
        var totalWeight = incompleteWeight + _finishedHypotheses.Select(e => e.Weight).Sum();
        return _finishedHypotheses
            .GroupBy(e => e.State.MayResult)
            .Select(e => new KeyValuePair<May<BigInteger>, BigRational>(e.Key, e.Select(f => f.Weight).Sum()/totalWeight))
            .Prepend(new KeyValuePair<May<BigInteger>, BigRational>(May.NoValue, incompleteWeight / totalWeight));
    }

    public void Measure(BigInteger measuredResult, BigInteger nextInput) {
        var remaining = _finishedHypotheses.Where(e => e.State.MayResult.ForceGetValue() == measuredResult).Concat(_runningHypotheses).ToArray();
        _runningHypotheses.Clear();
        _finishedHypotheses.Clear();
        foreach (var hypothesis in remaining) {
            _runningHypotheses.Enqueue(new Hypothesis(hypothesis.Prior, hypothesis.State == null ? null : new TuringMachine(hypothesis.State.Instructions, nextInput), hypothesis.MaxSeenElapsedSteps));
        }
    }

    public void Advance(int steps) {
        while (steps-- >= 0) {
            Advance();
        }
    }
    public void Advance() {
        var r = _runningHypotheses.MayDequeue().ForceGetValue();
        if (r.State == null) {
            _unexploredHypotheses.MoveNext();
            var newHypothesis = _unexploredHypotheses.Current;
            var unexploredPrior = r.Prior - newHypothesis.Prior;
            _runningHypotheses.Enqueue(newHypothesis);
            _runningHypotheses.Enqueue(new Hypothesis(unexploredPrior, null, 1));
            return;
        }

        var s2 = r.State.Advanced(100);
        var r2 = new Hypothesis(r.Prior, s2, r.MaxSeenElapsedSteps > s2.ElapsedSteps ? r.MaxSeenElapsedSteps : s2.ElapsedSteps);
        var output = r2.State.MayResult;
        if (output.HasValue) {
            _finishedHypotheses.Add(r2);
        } else if (!s2.IsStuckInALoop) {
            _runningHypotheses.Enqueue(r2);
        }
    }
}

