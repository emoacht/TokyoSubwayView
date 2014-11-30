
namespace TokyoSubwayView.Models.Metro
{
	public enum TrainState
	{
		/// <summary>
		/// Vacant (No train on a track) 
		/// </summary>
		Vacant = 0,

		/// <summary>
		/// Phantom (Train is expected to run on a track)
		/// </summary>
		Phantom = 1,

		/// <summary>
		/// On time
		/// </summary>
		OnTime = 2,

		/// <summary>
		/// Delayed for shorter time
		/// </summary>
		DelayShort = 3,

		/// <summary>
		/// Delayed for longer time
		/// </summary>
		DelayLong = 4,
	}
}