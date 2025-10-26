using UnityEngine;
using UnityEngine.Pool;

namespace Code.Presentation
{
    internal sealed class CardPool
    {
        private readonly GameObject _poolContainer;
        private readonly Transform _gameBoardParent;
        private readonly CardView _cardViewPrefab;
        private readonly ObjectPool<CardView> _cardPool;

        internal CardPool(CardView cardViewPrefab, Transform gameBoardParent)
        {
            _poolContainer = new GameObject(name: $"'{cardViewPrefab.name}' POOL");
            _cardViewPrefab = cardViewPrefab;
            _gameBoardParent = gameBoardParent;
            _cardPool = new ObjectPool<CardView>(CreateInstance, actionOnRelease: ReleaseInstance, actionOnGet: GetInstance, defaultCapacity: 15);
        }

        internal CardView Get() =>
            _cardPool.Get();

        internal void Release(CardView card) =>
            _cardPool.Release(card);

        private void GetInstance(CardView instance)
        {
            instance.transform.SetParent(_gameBoardParent.transform, worldPositionStays: false);
            instance.gameObject.SetActive(true);
        }

        private void ReleaseInstance(CardView instance)
        {
            instance.transform.SetParent(_poolContainer.transform, worldPositionStays: false);
            instance.gameObject.SetActive(false);
        }

        private CardView CreateInstance()
        {
            var card = Object.Instantiate(_cardViewPrefab, _poolContainer.transform, worldPositionStays: false);
            card.gameObject.SetActive(false);
            return card;
        }
    }
}