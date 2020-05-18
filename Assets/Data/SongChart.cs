using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Assets.Data
{
    public class SongChart
    {
        public int StepsPerBeat { get; private set; }

        public List<Tempo> TempoChanges { get; private set; }
        public List<TimeSignature> TimeSignatureChanges { get; private set; }
        public List<Section> Sections { get; private set; }
        public List<Note> Notes { get; private set; }

        public SongChart(int stepsPerBeat, List<TimeSignature> timeSignatureChanges, List<Tempo> tempoChanges, List<Section> sections, List<Note> notes)
        {
            StepsPerBeat = stepsPerBeat;

            // TODO: Ignoring time signature changes for now -- assume everything is 4/4
            //TimeSignatureChanges = timeSignatureChanges;

            TempoChanges = tempoChanges;

            // TODO: Ignoring sections for now
            //Sections = sections;

            Notes = notes;
        }

        public int GetPositionFromTime(double timeInSeconds)
        {
            int totalPositionInSteps = 0;

            double currentTimeInSeconds = 0;
            Tempo currentTempo = null;
            foreach (Tempo nextTempo in TempoChanges)
            {
                if (currentTempo != null)
                {
                    currentTimeInSeconds = GetTimeFromPosition(nextTempo.PositionInSteps);
                    if (timeInSeconds < currentTimeInSeconds)
                    {
                        break;
                    }

                    totalPositionInSteps = nextTempo.PositionInSteps;
                }

                currentTempo = nextTempo;
            }

            totalPositionInSteps += GetPositionFromTimeDelta(currentTimeInSeconds, timeInSeconds, currentTempo.BeatsPerMinute, StepsPerBeat);
            return totalPositionInSteps;
        }

        public double GetTimeFromPosition(int positionInSteps)
        {
            double totalTimeInSeconds = 0;

            Tempo currentTempo = null;
            foreach (Tempo nextTempo in TempoChanges)
            {
                if (currentTempo != null)
                {
                    if (nextTempo.PositionInSteps < positionInSteps)
                    {
                        break;
                    }

                    totalTimeInSeconds += GetTimeFromPositionDelta(currentTempo.PositionInSteps, nextTempo.PositionInSteps, currentTempo.BeatsPerMinute, StepsPerBeat);
                }

                currentTempo = nextTempo;
            }

            totalTimeInSeconds += GetTimeFromPositionDelta(currentTempo.PositionInSteps, positionInSteps, currentTempo.BeatsPerMinute, StepsPerBeat);
            return totalTimeInSeconds;
        }

        private double GetTimeFromPositionDelta(int startPosition, int endPosition, double beatsPerMinute, int stepsPerBeat)
        {
            return (endPosition - startPosition) / stepsPerBeat * 60 / beatsPerMinute;
        }

        private int GetPositionFromTimeDelta(double startTime, double endTime, double beatsPerMinute, int stepsPerBeat)
        {
            return (int)Math.Round((endTime - startTime) * beatsPerMinute / 60 * stepsPerBeat);
        }
    }
}
