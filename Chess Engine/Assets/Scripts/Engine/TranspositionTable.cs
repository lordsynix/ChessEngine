using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;

// Klasse für einen Eintrag in der Transpositionstabelle
public class Entry
{
    public ulong ZobristKey { get; set; }
    public int Evaluation { get; set; }
    public int Depth { get; set; }
    /*public EntryType Type { get; set; }

    // Definiert den Eintragstyp
    public enum EntryType
    {
        Exact, // "Exaktes" Ergebnis
        LowerBound, // Untere Schranke - bedeutet, dass die Position mindestens so schlecht ist
        UpperBound // Obere Schranke - bedeutet, dass die Position hoechstens so gut ist
    }*/
}

public class TranspositionTable
{
    private Dictionary<ulong, Entry> table;

    // Geschaetzte Groesse eines Eintrags in die Transpositionstabelle
    private int sizeOfEntry = sizeof(ulong) + 2 * sizeof(int);
    private long availableMemory;

    public TranspositionTable(int size)
    {
        // Limitiert die zwischengespeicherten Position auf einen begraenzten Speicherplatz
        table = new Dictionary<ulong, Entry>(size);
        availableMemory = size;
    }

    public void Store(ulong key, int evaluation, int depth)
    {
        table[key] = new Entry
        {
            ZobristKey = key,
            Evaluation = evaluation,
            Depth = depth
        };
        long usedMemory = sizeOfEntry * table.Count;
        double usagePercentage = Math.Round((double)usedMemory / availableMemory, 3);
        Diagnostics.Instance.UpdateTranspositionTableVisuals(usagePercentage, table.Count);
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
