﻿// *************************************************************************** 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>
// ***************************************************************************

using System.Collections.ObjectModel;
using JetBrains.Annotations;
using StateMachine.Events;

namespace StateMachine
{
    [PublicAPI]
    public class Transition<TState, TTrigger, TData>
    {
        private TransitionModel<TState, TTrigger> Model { get; set; }

        public TState Target => Model.Target;

        public bool Pop => Model.Pop;

        public TState Source
        {
            get { return Model.Source; }
            set { Model.Source = value; }
        }

        public Transition(TransitionModel<TState, TTrigger> model)
        {
            Model = model;
        }

        /// <exception cref="FsmBuilderException">When target is null</exception>
        public Transition(Collection<TTrigger> triggers, TState source, TState target)
        {
            if (target == null) throw FsmBuilderException.TargetStateCannotBeNull();

            Model = new TransitionModel<TState, TTrigger>(source, target);
            Model.Triggers.UnionWith(triggers);
        }

        /// <summary>
        ///     Generates a new Transition that will pop from the stack and therefore doesn't need a target.<br />
        ///     The target will vary depending on the items on the stack.
        /// </summary>
        /// <param name="triggers">The triggers.</param>
        /// <param name="source">The source.</param>
        public Transition(Collection<TTrigger> triggers, TState source)
            : this(triggers, source, default(TState))
        {
            Model.Pop = true;
        }

        /// <exception cref="FsmBuilderException">When target is null</exception>
        public Transition(TTrigger trigger, TState source, TState target)
            : this(new Collection<TTrigger> {trigger}, source, target)
        {
        }

        /// <summary>
        ///     Generates a new Transition that will pop from the stack and therefore doesn't need a target.<br />
        ///     The target will vary depending on the items on the stack.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <param name="source">The source.</param>
        public Transition(TTrigger trigger, TState source)
            : this(new Collection<TTrigger> {trigger}, source)
        {
        }

        /// <exception cref="FsmBuilderException">When trigger has been declared before</exception>
        public void Add(TTrigger trigger)
        {
            if (Model.Triggers.Contains(trigger)) throw FsmBuilderException.TriggerAlreadyDeclared(trigger);

            Model.Triggers.Add(trigger);
        }

        public bool Process(State<TState, TTrigger, TData> from, TTrigger input)
        {
            return Model.Triggers.Contains(input) &&
                   Model.Conditions.TrueForAll(x => x(new IfArgs<TState, TTrigger>(from.Identifier, Model.Target, input)));
        }

        public override string ToString()
        {
            return $"{Model.Source}-({Model.Triggers})->{Model.Target}";
        }
    }
}