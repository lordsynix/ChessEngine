using System;
using System.Collections.Generic;

/// <summary>
/// Die Klasse <c>Entry</c> stellt einen Eintrag in die Transpositionstabelle dar.
/// </summary>
public class Entry
{
    public ulong ZobristKey { get; set; }
    public int Evaluation { get; set; }
}

/// <summary>
/// Die Klasse <c>TranspositionTable</c> speichert unter einem Zobrist-Key (nahezu einzigartiger Hash 
/// einer Position) die Evaluation ab, um den Suchprozess bei repetitiven Brettstellungen zu beschleunigen.
/// </summary>
public class TranspositionTable
{
    private Dictionary<ulong, Entry> table;

    // Geschaetzte Groesse eines Eintrags in die Transpositionstabelle
    public static int SizeOfEntry = sizeof(ulong) + sizeof(int);
    public static long AvailableMemory = -1;
    public static int EntryCount = 0;

    public TranspositionTable(int size)
    {
        // Limitiert die zwischengespeicherten Position auf einen begraenzten Speicherplatz
        table = new Dictionary<ulong, Entry>(size);
        AvailableMemory = size;
    }

    /// <summary>
    /// Die Funktion <c>Store</c> speichert eine Positionsbeurteilung in der Transpositionstabelle.
    /// </summary>
    /// <param name="key">Zobrist-Hash der Position</param>
    /// <param name="evaluation">Evaluation der Position</param>
    public void Store(ulong key, int evaluation)
    {
        table[key] = new Entry
        {
            ZobristKey = key,
            Evaluation = evaluation
        };
        EntryCount++;
    }

    /// <summary>
    /// Die Funktion <c>Lookup</c> gibt die Bewertung fuer eine Position zurueck, falls diese bereits vorgekommen ist.
    /// </summary>
    /// <param name="key">Zobrist-Hash der Position</param>
    /// <returns>Gespeicherter Eintrag zu dieser Position</returns>
    public Entry Lookup(ulong key)
    {
        if (table.TryGetValue(key, out var entry))
        {
            return entry;
        }
        return null;
    }
}
