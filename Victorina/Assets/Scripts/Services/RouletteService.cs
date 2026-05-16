using System;
using System.Collections.Generic;
using UnityEngine;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;
using Random = System.Random;

namespace Victorina.Services
{
    public class RouletteService : IRouletteService
    {
        private readonly Random _rng = new Random();

        public int SelectSector(IReadOnlyList<int> poolIndices)
        {
            Debug.Log($"[RouletteService] SelectSector: категорий={poolIndices?.Count ?? 0}");
            if (poolIndices == null || poolIndices.Count == 0)
            {
                Debug.LogError("[RouletteService] ОШИБКА: список категорий пуст!");
                throw new InvalidOperationException("Нет доступных категорий.");
            }
            var selected = poolIndices[_rng.Next(poolIndices.Count)];
            Debug.Log($"[RouletteService] ✓ Выбрана категория: {selected} ({PoolInfo.Names[selected]})");
            return selected;
        }
    }
}
