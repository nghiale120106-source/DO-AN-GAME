using System;
using UnityEngine;

namespace DuaXe3D
{
    /// <summary>
    /// Tổng hợp nhạc nền + hiệu ứng âm thanh bằng code.
    /// Toàn bộ âm thanh là bản gốc, không dùng file có bản quyền.
    /// </summary>
    public static class SynthAudio
    {
        private const int SR = 44100;
        private static readonly float[] SinTable = BuildSinTable(4096);
        private static AudioClip _menuMusic;
        private static AudioClip _raceMusic;
        private static AudioClip _engine;
        private static AudioClip _click;
        private static AudioClip _whoosh;
        private static AudioClip _beep;
        private static AudioClip _go;
        private static AudioClip _win;
        private static AudioClip _pickup;
        private static AudioClip _crash;

        private enum Wave
        {
            Sine,
            Saw,
            Square,
            Triangle
        }

        private static float[] BuildSinTable(int size)
        {
            float[] t = new float[size];
            for (int i = 0; i < size; i++) t[i] = Mathf.Sin(i / (float)size * Mathf.PI * 2f);
            return t;
        }

        private static float SinCycles(float phase)
        {
            phase -= Mathf.Floor(phase);
            float f = phase * SinTable.Length;
            int i = (int)f;
            int j = (i + 1) % SinTable.Length;
            return Mathf.Lerp(SinTable[i], SinTable[j], f - i);
        }

        private static float Osc(Wave wave, float phase)
        {
            float p = phase - Mathf.Floor(phase);
            switch (wave)
            {
                case Wave.Sine: return SinCycles(phase);
                case Wave.Saw: return p * 2f - 1f;
                case Wave.Square: return p < 0.5f ? 1f : -1f;
                default: return 4f * Mathf.Abs(p - 0.5f) - 1f;
            }
        }

        private static float NoteFreq(int midi)
        {
            return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
        }

        private static AudioClip Make(string name, float[] data)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, SR, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip MenuMusic
        {
            get
            {
                if (_menuMusic == null) _menuMusic = BuildMusic(false);
                return _menuMusic;
            }
        }

        public static AudioClip RaceMusic
        {
            get
            {
                if (_raceMusic == null) _raceMusic = BuildMusic(true);
                return _raceMusic;
            }
        }

        public static AudioClip Engine
        {
            get
            {
                if (_engine == null) _engine = BuildEngine();
                return _engine;
            }
        }

        public static AudioClip Click
        {
            get
            {
                if (_click == null) _click = BuildClick();
                return _click;
            }
        }

        public static AudioClip Whoosh
        {
            get
            {
                if (_whoosh == null) _whoosh = BuildWhoosh();
                return _whoosh;
            }
        }

        public static AudioClip Beep
        {
            get
            {
                if (_beep == null) _beep = BuildBeep(false);
                return _beep;
            }
        }

        public static AudioClip GoBeep
        {
            get
            {
                if (_go == null) _go = BuildBeep(true);
                return _go;
            }
        }

        public static AudioClip Win
        {
            get
            {
                if (_win == null) _win = BuildWin();
                return _win;
            }
        }

        public static AudioClip Pickup
        {
            get
            {
                if (_pickup == null) _pickup = BuildPickup();
                return _pickup;
            }
        }

        public static AudioClip Crash
        {
            get
            {
                if (_crash == null) _crash = BuildCrash();
                return _crash;
            }
        }

        // ---------------------------------------------------------------- music

        private static AudioClip BuildMusic(bool race)
        {
            int bpm = race ? 128 : 92;
            int bars = race ? 8 : 4;
            double beat = 60.0 / bpm;
            double bar = beat * 4.0;
            double duration = bar * bars;
            int n = Mathf.CeilToInt((float)(duration * SR));
            float[] melodic = new float[n];
            float[] drums = new float[n];

            int[] roots = { 45, 41, 48, 43 };          // A2, F2, C3, G2
            int[][] chords =
            {
                new[] { 57, 60, 64 },                   // Am
                new[] { 53, 57, 60 },                   // F
                new[] { 60, 64, 67 },                   // C
                new[] { 55, 59, 62 }                    // G
            };

            System.Random rnd = new System.Random(race ? 9001 : 404);

            for (int b = 0; b < bars; b++)
            {
                int ci = b % 4;
                double t0 = b * bar;
                int[] chord = chords[ci];

                // Pad – 3 nốt ngân dài
                for (int c = 0; c < chord.Length; c++)
                {
                    AddNote(melodic, t0, bar * 0.96, NoteFreq(chord[c] + 12), race ? 0.085f : 0.10f,
                        Wave.Saw, 0.30f, 0.55f, 0.0015f, 1900f);
                    AddNote(melodic, t0, bar * 0.96, NoteFreq(chord[c] + 12), race ? 0.05f : 0.06f,
                        Wave.Sine, 0.35f, 0.55f, -0.0012f, 0f);
                }

                // Bass – nốt móc
                int bassRoot = roots[ci];
                for (int s = 0; s < 8; s++)
                {
                    int midi = bassRoot + ((s == 5 || s == 7) ? 12 : 0);
                    AddNote(melodic, t0 + s * beat * 0.5, beat * 0.44, NoteFreq(midi), 0.36f,
                        Wave.Saw, 0.006f, 0.09f, 0f, 340f);
                    AddNote(melodic, t0 + s * beat * 0.5, beat * 0.42, NoteFreq(midi - 12), 0.20f,
                        Wave.Sine, 0.006f, 0.09f, 0f, 0f);
                }

                // Arpeggio 16 nốt
                int[] pattern = { 0, 1, 2, 1 };
                for (int s = 0; s < 16; s++)
                {
                    int note = chord[pattern[s % 4]] + 24;
                    double at = t0 + s * beat * 0.25;
                    AddNote(melodic, at, beat * 0.20, NoteFreq(note), race ? 0.055f : 0.04f,
                        Wave.Square, 0.004f, 0.10f, 0f, 0f);
                    AddNote(melodic, at + 0.19, beat * 0.16, NoteFreq(note), race ? 0.018f : 0.015f,
                        Wave.Sine, 0.005f, 0.10f, 0f, 0f);
                }

                // Trống
                if (race)
                {
                    for (int beatIndex = 0; beatIndex < 4; beatIndex++)
                        AddKick(drums, t0 + beatIndex * beat, 0.85f, rnd);
                    AddSnare(drums, t0 + beat, 0.42f, rnd);
                    AddSnare(drums, t0 + beat * 3, 0.42f, rnd);
                    for (int h = 0; h < 8; h++)
                        AddHat(drums, t0 + h * beat * 0.5, h % 2 == 0 ? 0.10f : 0.055f, rnd);
                }
                else
                {
                    AddKick(drums, t0, 0.45f, rnd);
                    AddKick(drums, t0 + beat * 2, 0.32f, rnd);
                    AddHat(drums, t0 + beat * 2.5, 0.05f, rnd);
                    AddHat(drums, t0 + beat * 3.5, 0.04f, rnd);
                }
            }

            // Side-chain: nhấp nhô theo kick
            float[] pump = new float[n];
            for (int i = 0; i < n; i++) pump[i] = 1f;
            for (int b = 0; b < bars; b++)
            {
                double t0 = b * bar;
                int beats = race ? 4 : 2;
                for (int k = 0; k < beats; k++)
                {
                    double kt = t0 + k * beat;
                    int ks = (int)(kt * SR);
                    int len = (int)(0.26 * SR);
                    for (int i = 0; i < len && ks + i < n; i++)
                    {
                        float duck = 1f - 0.38f * Mathf.Exp(-i / (float)SR * 15f);
                        pump[ks + i] *= duck;
                    }
                }
            }

            float[] mix = new float[n];
            float peak = 0f;
            for (int i = 0; i < n; i++)
            {
                float v = melodic[i] * pump[i] + drums[i];
                v = v / (1f + Mathf.Abs(v)) * 1.55f;
                mix[i] = v;
                float a = Mathf.Abs(v);
                if (a > peak) peak = a;
            }

            float norm = peak > 0.001f ? 0.82f / peak : 1f;
            for (int i = 0; i < n; i++) mix[i] *= norm;
            ApplyEdgeFade(mix, 0.01f);
            return Make(race ? "RaceMusic" : "MenuMusic", mix);
        }

        private static void AddNote(float[] buf, double start, double dur, float freq, float gain,
            Wave wave, float attack, float release, float detune, float lowpass)
        {
            int s0 = (int)(start * SR);
            int len = (int)(dur * SR);
            if (s0 < 0) { len += s0; s0 = 0; }
            if (s0 >= buf.Length || len <= 0) return;
            len = Mathf.Min(len, buf.Length - s0);

            float phase = 0f;
            float phase2 = 0.17f;
            float lp = 0f;
            float cutoff = lowpass > 0f ? 1f - Mathf.Exp(-2f * Mathf.PI * lowpass / SR) : 1f;
            float atk = Mathf.Max(0.002f, Mathf.Min(attack, (float)dur * 0.4f));
            float rel = Mathf.Max(0.005f, Mathf.Min(release, (float)dur * 0.5f));

            for (int i = 0; i < len; i++)
            {
                float t = i / (float)SR;
                float env;
                if (t < atk) env = t / atk;
                else if (t > dur - rel) env = Mathf.Clamp01((float)((dur - t) / rel));
                else env = 1f;

                float v = Osc(wave, phase) * 0.72f + Osc(wave, phase2) * 0.28f;
                if (lowpass > 0f)
                {
                    lp += (v - lp) * cutoff;
                    v = lp;
                }
                buf[s0 + i] += v * env * gain;

                phase += freq / SR;
                phase2 += freq * (1f + detune) / SR;
                if (phase > 1000f) phase -= 1000f;
                if (phase2 > 1000f) phase2 -= 1000f;
            }
        }

        private static void AddKick(float[] buf, double start, float gain, System.Random rnd)
        {
            int s0 = (int)(start * SR);
            int len = (int)(0.30 * SR);
            if (s0 >= buf.Length) return;
            len = Mathf.Min(len, buf.Length - s0);
            float phase = 0f;
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)SR;
                float freq = 44f + 95f * Mathf.Exp(-t * 32f);
                phase += freq / SR;
                float env = Mathf.Exp(-t * 10.5f);
                float v = SinCycles(phase) * env;
                if (t < 0.006f) v += (1f - t / 0.006f) * 0.4f * (float)(rnd.NextDouble() * 2.0 - 1.0);
                buf[s0 + i] += v * gain;
            }
        }

        private static void AddSnare(float[] buf, double start, float gain, System.Random rnd)
        {
            int s0 = (int)(start * SR);
            int len = (int)(0.20 * SR);
            if (s0 >= buf.Length) return;
            len = Mathf.Min(len, buf.Length - s0);
            float prev = 0f;
            float phase = 0f;
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)SR;
                float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                float hp = noise - prev * 0.72f;
                prev = noise;
                float env = Mathf.Exp(-t * 24f);
                phase += 185f / SR;
                float body = SinCycles(phase) * Mathf.Exp(-t * 30f) * 0.35f;
                buf[s0 + i] += (hp * 0.62f + body) * env * gain;
            }
        }

        private static void AddHat(float[] buf, double start, float gain, System.Random rnd)
        {
            int s0 = (int)(start * SR);
            int len = (int)(0.06 * SR);
            if (s0 >= buf.Length) return;
            len = Mathf.Min(len, buf.Length - s0);
            float prev = 0f;
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)SR;
                float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                float hp = noise - prev;
                prev = noise;
                buf[s0 + i] += hp * Mathf.Exp(-t * 75f) * gain;
            }
        }

        private static void ApplyEdgeFade(float[] buf, float seconds)
        {
            int f = Mathf.Min(buf.Length / 2, (int)(seconds * SR));
            for (int i = 0; i < f; i++)
            {
                float k = i / (float)f;
                buf[i] *= k;
                buf[buf.Length - 1 - i] *= k;
            }
        }

        // ---------------------------------------------------------------- engine

        private static AudioClip BuildEngine()
        {
            int n = SR;
            float[] buf = new float[n];
            float p1 = 0f, p2 = 0f, p3 = 0f, p4 = 0f, pm = 0f;
            float lp = 0f;
            float lp2 = 0f;
            for (int i = 0; i < n; i++)
            {
                float saw1 = p1 * 2f - 1f;
                float sq = p2 < 0.5f ? 1f : -1f;
                float sine = SinCycles(p3);
                float sub = SinCycles(p4);

                float mod = 0.72f + 0.28f * SinCycles(pm);
                float v = saw1 * 0.34f + sq * 0.14f + sine * 0.20f + sub * 0.22f;
                v *= mod;

                lp += (v - lp) * 0.18f;
                lp2 += (lp - lp2) * 0.35f;
                buf[i] = lp2 * 0.9f;

                p1 += 55f / SR;
                p2 += 110f / SR;
                p3 += 165f / SR;
                p4 += 27.5f / SR;
                pm += 12.5f / SR;
                if (p1 > 1f) p1 -= 1f;
                if (p2 > 1f) p2 -= 1f;
                if (p3 > 1f) p3 -= 1f;
                if (p4 > 1f) p4 -= 1f;
                if (pm > 1f) pm -= 1f;
            }

            // Nối vòng mượt
            int fade = (int)(0.05f * SR);
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                buf[n - fade + i] = Mathf.Lerp(buf[n - fade + i], buf[i], k);
            }
            ApplyEdgeFade(buf, 0.003f);
            return Make("EngineLoop", buf);
        }

        // ---------------------------------------------------------------- sfx

        private static AudioClip BuildClick()
        {
            int n = (int)(0.08f * SR);
            float[] buf = new float[n];
            float p = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SR;
                float env = Mathf.Exp(-t * 55f);
                float freq = 900f + t * 12000f;
                p += freq / SR;
                buf[i] = SinCycles(p) * env * 0.35f;
            }
            return Make("UiClick", buf);
        }

        private static AudioClip BuildWhoosh()
        {
            int n = (int)(0.75f * SR);
            float[] buf = new float[n];
            System.Random rnd = new System.Random(77);
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SR;
                float k = t / 0.75f;
                float cutoff = Mathf.Lerp(0.32f, 0.04f, k);
                float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                lp += (noise - lp) * cutoff;
                float env = Mathf.Sin(Mathf.Clamp01(k) * Mathf.PI);
                buf[i] = lp * env * 0.65f;
            }
            return Make("Whoosh", buf);
        }

        private static AudioClip BuildBeep(bool go)
        {
            int n = (int)((go ? 0.65f : 0.18f) * SR);
            float[] buf = new float[n];
            float p1 = 0f, p2 = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SR;
                float env = Mathf.Min(1f, t * 80f) * Mathf.Exp(-t * (go ? 3.0f : 7f));
                float f1 = go ? 880f : 660f;
                float f2 = go ? 1320f : 990f;
                p1 += f1 / SR;
                p2 += f2 / SR;
                buf[i] = (SinCycles(p1) * 0.55f + SinCycles(p2) * 0.3f) * env * 0.5f;
            }
            return Make(go ? "GoBeep" : "CountdownBeep", buf);
        }

        private static AudioClip BuildWin()
        {
            int n = (int)(3.0f * SR);
            float[] buf = new float[n];
            int[] notes = { 72, 76, 79, 84, 88 };
            for (int i = 0; i < notes.Length; i++)
            {
                double at = i * 0.13;
                AddNote(buf, at, 0.42, NoteFreq(notes[i]), 0.26f, Wave.Saw, 0.005f, 0.18f, 0.001f, 5000f);
                AddNote(buf, at, 0.42, NoteFreq(notes[i] + 12), 0.10f, Wave.Sine, 0.005f, 0.18f, 0f, 0f);
            }
            int[] chord = { 72, 76, 79, 84 };
            for (int c = 0; c < chord.Length; c++)
            {
                AddNote(buf, 0.70, 2.1, NoteFreq(chord[c]), 0.18f, Wave.Saw, 0.02f, 1.4f, 0.0015f, 4200f);
                AddNote(buf, 0.70, 2.1, NoteFreq(chord[c] + 12), 0.06f, Wave.Sine, 0.02f, 1.4f, 0f, 0f);
            }
            for (int i = 0; i < 24; i++)
            {
                double at = 0.7 + i * 0.055;
                AddNote(buf, at, 0.5, NoteFreq(96 + (i * 7) % 12), 0.035f, Wave.Sine, 0.005f, 0.4f, 0f, 0f);
            }
            Normalize(buf, 0.75f);
            ApplyEdgeFade(buf, 0.01f);
            return Make("Victory", buf);
        }

        private static AudioClip BuildPickup()
        {
            int n = (int)(0.42f * SR);
            float[] buf = new float[n];
            float p = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SR;
                float f = 520f + 1200f * (t / 0.42f);
                p += f / SR;
                float env = Mathf.Exp(-t * 7f);
                buf[i] = SinCycles(p) * env * 0.35f;
                buf[i] += SinCycles(p * 2.01f) * env * 0.12f;
            }
            return Make("NitroPickup", buf);
        }

        private static AudioClip BuildCrash()
        {
            int n = (int)(0.45f * SR);
            float[] buf = new float[n];
            System.Random rnd = new System.Random(123);
            float lp = 0f;
            float p = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SR;
                float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                lp += (noise - lp) * 0.22f;
                p += 70f / SR;
                float env = Mathf.Exp(-t * 13f);
                buf[i] = (lp * 0.6f + SinCycles(p) * 0.35f) * env * 0.6f;
            }
            return Make("Crash", buf);
        }

        private static void Normalize(float[] buf, float target)
        {
            float peak = 0f;
            for (int i = 0; i < buf.Length; i++)
            {
                float a = Mathf.Abs(buf[i]);
                if (a > peak) peak = a;
            }
            if (peak < 0.0001f) return;
            float k = target / peak;
            for (int i = 0; i < buf.Length; i++) buf[i] *= k;
        }
    }
}
