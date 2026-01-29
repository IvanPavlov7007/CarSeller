using System.Collections;
using UnityEngine;

public class WarehouseEnterProcess : IProcess
{
    public readonly Car Car;
    public readonly Warehouse Warehouse;

    public WarehouseEnterProcess(Car car, Warehouse warehouse)
    {
        Car = car;
        Warehouse = warehouse;
    }

    public IEnumerator Run()
    {
        var offer = G.CarWarehousePolicy.Resolve(Car, Warehouse, new OperationContext());
        switch (offer)
        {
            case PutCarInWarehouseOffer carOffer:
                yield return puttingWholeCar(carOffer);
                break;
            case WarehouseStripCarOffer stripOffer:
                yield return strippingCar(stripOffer);
                break;
            default:
                Debug.LogWarning("No appropriate offer created by warehouse policy");
                break;
        }
        yield break;
    }

    IEnumerator puttingWholeCar(PutCarInWarehouseOffer carOffer)
    {
        bool done = false; bool accepted = false;
        var element = CTX_Menu_Tools.PutCarInsideWarehoseConfirmation(
        carOffer,
        onAccept: () => { accepted = true; done = true; },
        onCancel: () => { done = true; });

        var menu = FixedContextMenuManager.Instance.CreateContextMenu(element);

        // If the menu closes for any other reason (e.g., blocker close), also proceed.
        menu.Closed += _ => done = true;

        yield return new WaitUntil(() => done);

        if (!accepted)
            yield break;

        // confirmed -> put car inside
        var transaction = carOffer.Accept();
        var result = G.TransactionProcessor.Process(transaction);
        if (result.Type != TransactionResultType.Success)
        {
            Debug.LogError("Failed to put car inside warehouse: " + result.Type);
        }
    }

    IEnumerator strippingCar(WarehouseStripCarOffer stripOffer)
    {
        //bring up confirmation window
        bool done = false; bool accepted = false;
        var element = CTX_Menu_Tools.StripCarInsideWarehoseConfirmation(
        stripOffer,
        onAccept: () => { accepted = true; done = true; },
        onCancel: () => { done = true; });

        var menu = FixedContextMenuManager.Instance.CreateContextMenu(element);
        menu.Closed += _ => done = true;

        yield return new WaitUntil(() => done);
        if (!accepted)
            yield break;
        //if confirmed open Warehouse Scene
        G.GameFlowController.EnterWarehouse(Warehouse);

        yield return new WaitUntil(() => G.GameFlowController.CurrentSceneType == GameFlowController.GameSceneType.Warehouse);
        yield return new WaitForSeconds(0.2f);

        var transaction = stripOffer.Accept();
        var result = G.TransactionProcessor.Process(transaction);
        if (result.Type != TransactionResultType.Success)
        {
            Debug.LogError("Failed to strip car inside warehouse: " + result.Type);
            yield break;
        }

        //open results window with claim button
        var stripData = transaction.Data as StripCarTransactionData;
        element = CTX_Menu_Tools.StipReslutsClaim(stripData.StrippingProcess.StrippedParts);
        menu = FixedContextMenuManager.Instance.CreateContextMenu(element);
        //on claim put things inside
    }
}