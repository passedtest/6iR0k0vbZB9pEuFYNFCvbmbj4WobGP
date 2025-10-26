using UnityEngine;
using UnityEngine.Pool;

namespace Code.Presentation
{
    internal sealed class CardPool
    {
        private readonly GameObject _poolContainer;
        private readonly Transform _gameBoardParent;
        private readonly Card _cardPrefab;
        private readonly ObjectPool<Card> _cardPool;

        internal CardPool(Card cardPrefab, Transform gameBoardParent)
        {
            _poolContainer = new GameObject(name : $"'{cardPrefab.name}' POOL");
            _cardPrefab = cardPrefab;
            _gameBoardParent = gameBoardParent;
            _cardPool = new ObjectPool<Card>(CreateInstance, actionOnRelease: ReleaseInstance, actionOnGet: GetInstance, defaultCapacity: 15);
        }

        internal Card Get() => 
            _cardPool.Get();
        
        internal void Release(Card card) => 
            _cardPool.Release(card);

        private void GetInstance(Card instance)
        {
            instance.transform.SetParent(_gameBoardParent.transform, worldPositionStays: false);
            instance.gameObject.SetActive(true);
        }

        private void ReleaseInstance(Card instance)
        {
            instance.transform.SetParent(_poolContainer.transform, worldPositionStays: false);
            instance.gameObject.SetActive(false);
        }

        private Card CreateInstance()
        {
            var card = Object.Instantiate(_cardPrefab, _poolContainer.transform, worldPositionStays: false);
            card.gameObject.SetActive(false);
            return card;
        }
    }
}