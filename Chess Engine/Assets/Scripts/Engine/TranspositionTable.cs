using System;
using System.Collections.Generic;

// Klasse für einen Eintrag in der Transpositionstabelle
public class Entry
{
    public ulong ZobristKey { get; set; }
    public int Evaluation { get; set; }
    public int Depth { get; set; }
}

public class TranspositionTable
{
    private Dictionary<ulong, Entry> table;

    // Geschaetzte Groesse eines Eintrags in die Transpositionstabelle
    public static int SizeOfEntry = sizeof(ulong) + 2 * sizeof(int);
    public static long AvailableMemory = -1;
    public static int EntryCount = 0;

    public TranspositionTable(int size)
    {
        // Limitiert die zwischengespeicherten Position auf einen begraenzten Speicherplatz
        table = new Dictionary<ulong, Entry>(size);
        AvailableMemory = size;
    }

    public void Store(ulong key, int evaluation, int depth)
    {
        table[key] = new Entry
        {
            ZobristKey = key,
            Evaluation = evaluation,
            Depth = depth
        };
        EntryCount++;
    }

    public Entry Lookup(ulong key)
    {
        if (table.TryGetValue(key, out var entry))
        {
            return entry;
        }
        return null;
    }
}
