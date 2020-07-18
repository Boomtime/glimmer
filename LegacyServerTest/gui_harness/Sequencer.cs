namespace ShadowCreatures.Glimmer {
	using System;
	using System.IO;

	public interface ISequence {
		void Execute();

		void ButtonStateChanged( IGlimDevice src, ButtonStatus btn );

		/// <summary>set current sequence time, when set it must not go backwards!</summary>
		TimeSpan CurrentTime { get; set; }

		/// <summary>global hack</summary>
		double Luminance { 
			set;
		}
		/// <summary>global hack</summary>
		double Saturation {
			set;
		}
	}

	abstract class SequenceDefault : ISequence {
		public virtual double Luminance {
			set;
			protected get;
		}
		public virtual double Saturation {
			set;
			protected get;
		}

		public virtual void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
		}

		public abstract void Execute();

		public TimeSpan CurrentTime { get; set; }

		protected IFxContext MakeCurrentContext() {
			return new FxContextSimple( CurrentTime );
		}
	}

	class SequenceTimeline : ISequence {
		public TimeSpan CurrentTime { get; set; }

		public double Luminance { set; private get; }
		public double Saturation { set; private get; }

		public void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
		}

		public void Execute() {
		}
	}

	static class Sequencer {

		static public ISequence Load( Stream file ) {
			return null;
		}
	}
}
