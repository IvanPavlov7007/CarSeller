public interface IProductViewBuilder<T>
{
    T BuildCar(Car car);
    T BuildWheel(Wheel wheel);
    T BuildEngine(Engine engine);
    T BuildSpoiler(Spoiler spoiler);
    T BuildCarFrame(CarFrame carFrame);
}