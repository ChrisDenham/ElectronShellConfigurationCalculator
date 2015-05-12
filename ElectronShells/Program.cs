/*
 * Electron configuration calculator.
 * https://github.com/ChrisDenham/ElectronShellConfigurationCalculator
 * Feel free to use or abuse without restriction.
 * May 2015
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronShells
{
    class Subshell : IComparable<Subshell>
    {
        public Subshell(int n, int l)
        {
            this.n = n;
            this.l = l;
        }

        override public string ToString()
        {
            return n.ToString() + subshellCode[l].ToString(); 
        }

        // Number of orbit configurations possible for this subshell.
        public int getOrbits() 
        { 
            return l * 2 + 1; 
        }

        // One pair of electrons allowed per orbit.
        public int getMaxElectrons()
        {
            return getOrbits() * 2;
        }

        public int CompareTo(Subshell compare)
        {
            // Subshell energy order is given by (l + n)
            // For equal (l + n), these are ordered by n.
            // See http://en.wikipedia.org/wiki/Electron_configuration#Atoms:_Aufbau_principle_and_Madelung_rule
            int diff = getEnergyOrder() - compare.getEnergyOrder();
            if (diff == 0)
            {
                return n - compare.n;
            }
            return diff;
        }

        // Used for sorting subshells into energy order.
        private int getEnergyOrder()
        {
            return l + n;
        }

        public static char[] subshellCode = { 's', 'p', 'd', 'f', 'g', 'h', 'i' };
        public int n;
        public int l;
    }

    class SubshellUsage : IComparable<SubshellUsage>
    {
        public SubshellUsage(Subshell subshell, int electrons)
        {
            this.subshell = subshell;
            this.electrons = electrons;
        }

        override public string ToString()
        {
            return subshell.ToString() + electrons.ToString();
        }

        public int CompareTo(SubshellUsage compare)
        {
            // To sort config by shell then subshell (i.e. instead of by energy)
            int diffShell = subshell.n - compare.subshell.n;
            if (diffShell == 0)
            {
                return subshell.l - compare.subshell.l;
            }
            return diffShell;
        }

        public Subshell subshell;
        public int electrons;
    }

    class ElectronConfigurationBuilder
    {
        public ElectronConfigurationBuilder()
        {
            // Construct a list of subshells for the 
            // first 7 shells and sort them by energy.
            subshells = new List<Subshell>();
            for (int n = 1; n < 8; ++n)
            {
                for (int l = 0; l < n; ++l)
                {
                    subshells.Add(new Subshell(n, l));
                }
            }
            subshells.Sort();
        }

        public List<SubshellUsage> getElectronConfiguration(int atomicNumber)
        {
            List<SubshellUsage> configuration = new List<SubshellUsage>();
            int remainingElectrons = atomicNumber;
            foreach (Subshell subshell in subshells)
            {
                if (remainingElectrons == 0)
                {
                    break;
                }

                int maxElectrons = subshell.getMaxElectrons();
                int usedElectrons = 0;
                if (remainingElectrons > maxElectrons)
                {
                    usedElectrons = maxElectrons;
                }
                else
                {
                    usedElectrons = remainingElectrons;
                }

                remainingElectrons = remainingElectrons - usedElectrons;

                configuration.Add(new SubshellUsage(subshell, usedElectrons));
            }

            if (remainingElectrons != 0)
            {
                throw new Exception("Error computing electron configuration");
            }

            // To sort by shell then subshell rather than by energy
            configuration.Sort();

            return configuration;
        }

        public string getElectronConfigurationString(int atomicNumber)
        {
            List<SubshellUsage> configuration = getElectronConfiguration(atomicNumber);
            string config = "";
            foreach (SubshellUsage usage in configuration)
            {
                config = config + usage + " ";
            }
            return config;
        }

        public string getNobleRelativeElectronConfigurationString(int atomicNumber)
        {
            List<SubshellUsage> configuration = getElectronConfiguration(atomicNumber);
            int nobleNumber = 0;
            string nobleName = "";
            if (atomicNumber > 86) { nobleNumber = 86; nobleName = "[Rn] "; } else   
            if (atomicNumber > 54) { nobleNumber = 54; nobleName = "[Xe] "; } else   
            if (atomicNumber > 36) { nobleNumber = 36; nobleName = "[Kr] "; } else   
            if (atomicNumber > 18) { nobleNumber = 18; nobleName = "[Ar] "; } else   
            if (atomicNumber > 10) { nobleNumber = 10; nobleName = "[Ne] "; } else   
            if (atomicNumber >  2) { nobleNumber =  2; nobleName = "[He] "; }
            
            if (nobleNumber != 0)
            {
                // Remove the shell configuration belonging to the largest noble 
                // element smaller than atomicNumber.
                // Note that we need the double loop search here because the
                // returned configurations are not sorted by energy and thus we
                // can't rely on parts of the two configurations not being interleaved.
                List<SubshellUsage> nobleConfiguration = getElectronConfiguration(nobleNumber);
                foreach (SubshellUsage nobelShell in nobleConfiguration)
                {
                    foreach (SubshellUsage usage in configuration)
                    {
                        if (nobelShell.CompareTo(usage) == 0)
                        {
                            configuration.Remove(usage);
                            break;
                        }
                    }
                }
            }

            string config = nobleName;
            foreach (SubshellUsage usage in configuration)
            {
                config = config + usage + " ";
            }
            return config;
        }

        public List<int> getShellOccupancy(int atomicNumber)
        {
            List<SubshellUsage> configuration = getElectronConfiguration(atomicNumber);
            int[] occupancies = { 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (SubshellUsage usage in configuration)
            {
                occupancies[usage.subshell.n] += usage.electrons;
            }
            List<int> list = new List<int>();
            for (int i = 1; i < occupancies.Length; ++i)
            {
                if (occupancies[i] == 0) break;
                list.Add(occupancies[i]);
            }
            return list;
        }

        public string getShellOccupancyString(int atomicNumber)
        {
            string list = "";
            List<int> occupancies = getShellOccupancy(atomicNumber);
            bool first = true;
            foreach (int electrons in occupancies)
            {
                if (!first) list += ",";
                first = false;
                list += electrons;
            }
            return "(" + list + ")";
        }
 
        private List<Subshell> subshells;
    }

    class Program
    {
        static void Main(string[] args)
        {
            ElectronConfigurationBuilder builder = new ElectronConfigurationBuilder();
            for (int atomicNumber = 1; atomicNumber <= 118; ++atomicNumber)
            {
                string config = builder.getElectronConfigurationString(atomicNumber);
                Console.WriteLine(atomicNumber.ToString().PadLeft(3) + ". " + config);
                Console.WriteLine("     " + builder.getNobleRelativeElectronConfigurationString(atomicNumber));
                Console.WriteLine("     " + builder.getShellOccupancyString(atomicNumber));
            }

            int [,] table = new int [8,19];
            for (int group = 0; group <= 18; ++group) table[0, group] = group;
            for (int period = 0; period <= 7; ++period) table[period, 0] = period;

            for (int atomicNumber = 1; atomicNumber <= 118; ++atomicNumber)
            {
                if (atomicNumber >= 57 && atomicNumber <= 71) continue; // exclude Lanthanides for now
                if (atomicNumber >= 89 && atomicNumber <= 103) continue; // exclude Actinides for now
                List<int> occupancy = builder.getShellOccupancy(atomicNumber);
                List<SubshellUsage> configuration = builder.getElectronConfiguration(atomicNumber);
                int period = occupancy.Count;
                int group = occupancy[occupancy.Count - 1];
                if (atomicNumber == 2) group = 18; // special case for helium
                if (period == 2 && configuration.Count > 2) group = group + 10; // shift period 2 elements with 2p subshell to RHS
                if (period == 3 && configuration.Count > 4) group = group + 10; // shift period 3 elements with 3p subshell to RHS
                if (period >= 4)
                {
                    // Add in the electrons from the d subshell below outer shell
                    foreach (SubshellUsage usage in configuration)
                    {
                        if (usage.subshell.n == period - 1 && usage.subshell.l == 2)
                        {
                            group = group + usage.electrons;
                        }
                    }
                }
                table[period, group] = atomicNumber;
            }

            Console.WriteLine();
            Console.WriteLine("Periodic table of elements");

            for (int period = 0; period <= 7; ++period)
            {
                for (int group = 0; group <= 18; ++group)
                {
                    int value = table[period, group];
                    Console.Write((value == 0 ? "" : value.ToString()).PadLeft(4));
                }
                Console.WriteLine();
            }

            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }
    }
}
