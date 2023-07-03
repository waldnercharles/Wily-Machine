using System;
using System.Collections.Generic;

namespace WilyMachine;

public class StateMachine
{
    private readonly Dictionary<Type, State> m_States = new Dictionary<Type, State>();

    private bool m_Enabled = true;

    public bool Enabled
    {
        get => m_Enabled;
        set
        {
            m_Enabled = value;

            if (!m_Enabled)
            {
                State = null;
            }
        }
    }

    public State? NextState { get; private set; }
    public State? State { get; private set; }

    public void Update(double delta)
    {
        if (!Enabled)
        {
            return;
        }

        var stateChangeAction = State?.Update((float)delta) ?? StateChange.Next;

        if (stateChangeAction == StateChange.Next && NextState != null)
        {
            var nextState = NextState;
            NextState = null;

            SetState(nextState);
        }
    }

    public void AddState<T>() where T : State, new()
    {
        m_States[typeof(T)] = new T();
    }

    public void AddState(State state)
    {
        m_States[state.GetType()] = state;
    }

    public void SetNextState<T>() where T : State
    {
        Log.Assert(m_States.ContainsKey(typeof(T)),
            "StateMachine does not contain state {0}", typeof(T).Name);

        SetNextState(m_States[typeof(T)]);
    }

    public void SetNextState(State state)
    {
        Log.Assert(m_States.ContainsValue(state),
            "StateMachine does not contain state {0}", state.GetType().Name);

        NextState = state;
    }

    public void SetState<T>() where T : State
    {
        Log.Assert(m_States.ContainsKey(typeof(T)),
            "StateMachine does not contain state {0}", typeof(T).Name);

        SetState(m_States[typeof(T)]);
    }

    public void SetState(State state)
    {
        if (!Enabled)
        {
            return;
        }

        Log.Assert(m_States.ContainsValue(state),
            "StateMachine does not contain state {0}", state.GetType().Name);

        var previousState = State;

        if (State?.CanExit() ?? true)
        {
            State?.Exit();

            State = state;
            State.Enter(previousState);
            Log.Trace("Entered State {0}", state.GetType().Name);
        }
    }
}
