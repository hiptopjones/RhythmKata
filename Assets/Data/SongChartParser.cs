using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Data
{
    public class SongChartParser
    {
        private string ChartFilePath { get; set; }

        private int StepsPerBeat { get; set; }
        private List<Tempo> TempoChanges { get; set; } = new List<Tempo>();
        private List<TimeSignature> TimeSignatureChanges { get; set; } = new List<TimeSignature>();
        private List<Section> Sections { get; set; } = new List<Section>();
        private List<Note> Notes { get; set; } = new List<Note>();

        public SongChartParser(string chartFilePath)
        {
            if (string.IsNullOrEmpty(chartFilePath))
            {
                throw new Exception($"Null or empty value for chart file path");
            }

            ChartFilePath = chartFilePath;
        }

        public SongChart ParseChart()
        {
            ClearChartData();

            string[] chartLines = File.ReadAllLines(ChartFilePath);

            string sectionName = null;
            List<string> sectionDataLines = new List<string>();
            bool isReadingSectionDataLines = false;

            foreach (string chartLine in chartLines)
            {
                string trimmedLine = chartLine?.Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    continue;
                }

                if (trimmedLine.StartsWith("["))
                {
                    Match match = Regex.Match(trimmedLine, @"^\[(?<SectionName>\w+)\]$");
                    if (!match.Success)
                    {
                        throw new Exception($"Unable to parse chart line: '{trimmedLine}'");
                    }

                    sectionName = match.Groups["SectionName"].Value;
                }
                else if (trimmedLine == "{")
                {
                    isReadingSectionDataLines = true;
                }
                else if (trimmedLine == "}")
                {
                    ParseSectionData(sectionName, sectionDataLines);

                    isReadingSectionDataLines = false;
                    sectionName = null;
                    sectionDataLines.Clear();
                }
                else
                {
                    if (isReadingSectionDataLines)
                    {
                        sectionDataLines.Add(trimmedLine);
                    }
                }
            }

            return new SongChart(StepsPerBeat, TimeSignatureChanges, TempoChanges, Sections, Notes);
        }

        private void ParseSectionData(string sectionName, List<string> sectionDataLines)
        {
            switch (sectionName)
            {
                case "Song":
                    ParseSongSectionData(sectionDataLines);
                    break;

                case "SyncTrack":
                    ParseSyncTrackSectionData(sectionDataLines);
                    break;

                case "Events":
                    ParseEventsSectionData(sectionDataLines);
                    break;

                case "ExpertDrums":
                    ParseNotesSectionData(sectionDataLines);
                    break;

                default:
                    throw new Exception($"Unexpected section name: '{sectionName}'");
            }
        }

        private void ParseSongSectionData(List<string> sectionDataLines)
        {
            foreach (string sectionDataLine in sectionDataLines)
            {
                KeyValuePair<string, string> keyValuePair = ParseSectionDataLine(sectionDataLine);
                switch (keyValuePair.Key)
                {
                    case "Resolution":
                        StepsPerBeat = Convert.ToInt32(keyValuePair.Value);
                        break;

                    default:
                        Debug.Log($"Ignoring Song section data: '{keyValuePair.Key}'");
                        break;
                }
            }
        }

        private void ParseSyncTrackSectionData(List<string> sectionDataLines)
        {
            foreach (string sectionDataLine in sectionDataLines)
            {
                KeyValuePair<string, string> keyValuePair = ParseSectionDataLine(sectionDataLine);

                int position = Convert.ToInt32(keyValuePair.Key);
                ParseSyncTrackDirective(position, keyValuePair.Value);
            }
        }

        private void ParseSyncTrackDirective(int position, string directive)
        {
            KeyValuePair<string, string> keyValuePair = ParseSectionDataDirective(directive);
            switch (keyValuePair.Key)
            {
                case "TS":
                    ParseTimeSignature(position, keyValuePair.Value);
                    break;

                case "B":
                    ParseTempo(position, keyValuePair.Value);
                    break;

                default:
                    Debug.Log($"Ignoring SyncTrack directive: '{directive}'");
                    break;
            }
        }

        private void ParseTimeSignature(int position, string timeSignatureValue)
        {
            Match match = Regex.Match(timeSignatureValue, @"^(?<Numerator>\d+)(\s+(?<DenominatorExponent>\d+))?");
            if (!match.Success)
            {
                throw new Exception($"Unable to parse time signature value: '{timeSignatureValue}'");
            }

            int numerator = Convert.ToInt32(match.Groups["Numerator"].Value);

            int denominator = 4;
            if (match.Groups["DenominatorExponent"].Success)
            {
                denominator = (int)Math.Pow(2, Convert.ToInt32(match.Groups["DenominatorExponent"].Value));
            }

            TimeSignatureChanges.Add(new TimeSignature(position, numerator, denominator));
        }

        private void ParseTempo(int position, string tempoValue)
        {
            double beatsPerMinute = Convert.ToInt32(tempoValue) / 1000d;

            TempoChanges.Add(new Tempo(position, beatsPerMinute));
        }

        private void ParseEventsSectionData(List<string> sectionDataLines)
        {
            foreach (string sectionDataLine in sectionDataLines)
            {
                KeyValuePair<string, string> keyValuePair = ParseSectionDataLine(sectionDataLine);

                int position = Convert.ToInt32(keyValuePair.Key);
                ParseEventsDirective(position, keyValuePair.Value);
            }
        }

        private void ParseEventsDirective(int position, string directive)
        {
            KeyValuePair<string, string> keyValuePair = ParseSectionDataDirective(directive);
            switch (keyValuePair.Key)
            {
                case "E":
                    ParseEvent(position, keyValuePair.Value);
                    break;

                default:
                    Debug.Log($"Ignoring Events directive: '{directive}'");
                    break;
            }
        }

        private void ParseEvent(int position, string eventValue)
        {
            Match match = Regex.Match(eventValue, @"^""section (?<Section>[^""]+)""$");
            if (!match.Success)
            {
                throw new Exception($"Unable to parse event: '{eventValue}'");
            }

            string sectionName = match.Groups["Section"].Value;

            Sections.Add(new Section(position, sectionName));
        }

        private void ParseNotesSectionData(List<string> sectionDataLines)
        {
            foreach (string sectionDataLine in sectionDataLines)
            {
                KeyValuePair<string, string> keyValuePair = ParseSectionDataLine(sectionDataLine);

                int position = Convert.ToInt32(keyValuePair.Key);
                ParseNotesDirective(position, keyValuePair.Value);
            }
        }

        private void ParseNotesDirective(int position, string directive)
        {
            KeyValuePair<string, string> keyValuePair = ParseSectionDataDirective(directive);
            switch (keyValuePair.Key)
            {
                case "N":
                    ParseNote(position, keyValuePair.Value);
                    break;

                default:
                    Debug.Log($"Ignoring Notes directive: '{directive}'");
                    break;
            }
        }

        private void ParseNote(int position, string noteValue)
        {
            Match match = Regex.Match(noteValue, @"^(?<NoteType>\d+) (?<NoteDuration>\d+)");
            if (!match.Success)
            {
                throw new Exception($"Unable to parse note value: '{noteValue}'");
            }

            int noteType = Convert.ToInt32(match.Groups["NoteType"].Value);
            int noteDuration = Convert.ToInt32(match.Groups["NoteDuration"].Value);

            Notes.Add(new Note(position, noteType, noteDuration));
        }

        private KeyValuePair<string, string> ParseSectionDataLine(string sectionDataLine)
        {
            Match match = Regex.Match(sectionDataLine, @"^(?<Key>[^=]+)=(?<Value>.*)$");
            if (match.Success)
            {
                string key = match.Groups["Key"].Value.Trim();
                string value = match.Groups["Value"].Value.Trim();

                return new KeyValuePair<string, string>(key, value);
            }

            throw new Exception($"Unable to parse section data line: '{sectionDataLine}'");
        }

        private KeyValuePair<string, string> ParseSectionDataDirective(string directive)
        {
            string[] directiveParts = directive.Split(new[] { ' ' }, 2);
            if (directiveParts.Length < 2)
            {
                throw new Exception($"Unable to parse directive: '{directive}'");
            }

            string directiveKey = directiveParts[0].Trim();
            string directiveValue = directiveParts[1].Trim();

            return new KeyValuePair<string, string>(directiveKey, directiveValue);
        }

        private void ClearChartData()
        {
            StepsPerBeat = 0;
            TempoChanges.Clear();
            TimeSignatureChanges.Clear();
            Sections.Clear();
            Notes.Clear();
        }
    }
}