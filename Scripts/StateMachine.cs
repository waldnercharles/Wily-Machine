using System;
using System.Collections.Generic;
using Godot;

namespace Spaghetti.Godot;

public partial class StateMachine : Node
{
    [Signal] public delegate void StateChangedEventHandler(State state);

    [Export] public Label? StateLabel { get; set; }

    [Export] public State? InitialState { get; set; }

    public State? State { get; private set; }

    public bool Enabled
    {
        get => m_Enabled;
        set
        {
            m_Enabled = value;
            SetPhysicsProcess(value);
            SetProcessInput(value);

            if (!m_Enabled)
            {
                State = null;
            }
        }
    }

    private bool m_Enabled;
    private Dictionary<Type, State> m_States = new Dictionary<Type, State>();

    public override void _Ready()
    {
        Enabled = true;
        State = InitialState;

        foreach (var child in GetChildren())
        {
            if (child is State childState)
            {
                childState.StateMachine = this;
                m_States[childState.GetType()] = childState;
            }
        }

        State?.Enter<State>(null);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        State?.HandleInput(@event);
    }

    public override void _PhysicsProcess(double delta)
    {
        State?.Update((float)delta);
    }

    public virtual void ChangeState<T>() where T : State
    {
        State?.Exit();

        if (m_States.TryGetValue(typeof(T), out var state))
        {
            var previousState = State;

            State = state;
            state.Enter(previousState);


            if (StateLabel != null)
            {
                StateLabel.Text = State.Name;
            }

            EmitSignal(SignalName.StateChanged, State);
        }
    }

    public virtual void AnimationFinished(StringName animationName)
    {
        State?.AnimationFinished(animationName);
    }
}
