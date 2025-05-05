import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import CurrentHitsSVG from './assets/CurrentHits.svg';

import { useAppDispatch, useAppSelector } from '../../hooks';
import { selectCurrentHP, selectMaxHP } from '../../store/selectors/sheet1Selectors';
import { updateCurrentHP, updateMaxHP } from '../../store/sheet1Slice';

const CurrentHitsCard: React.FC = () => {
  const dispatch = useAppDispatch();

  const maxHP = useAppSelector(selectMaxHP);
  const currentHP = useAppSelector(selectCurrentHP);

  const handleMaxHPChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateMaxHP(newValue));
    }
  };

  const handleCurrentHPChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateCurrentHP(newValue));
    }
  };

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={CurrentHitsSVG} />
      <Card.ImgOverlay className="p-1 m-0 d-flex flex-column justify-content-between text-center text-truncate h-100">
        <Form.Group
          controlId="characterMaxHP"
          className="d-flex flex-row w-100"
        >
          <Form.Label
            className="p-2 pt-1 pb-0 m-0 text-uppercase fw-lighter"
          >
            Максимум хитов
          </Form.Label>
          <Form.Control
            type="number"
            value={maxHP}
            className="p-2 pt-1 pb-0 m-0 bg-transparent border-0 border-bottom shadow-none text-center"
            onChange={handleMaxHPChange}
          />

        </Form.Group>
        <Form.Group
          controlId="characterCurrentHP"
          className="p-0 m-0 flex-grow-1 d-flex flex-column justify-content-between text-center text-truncate"
        >
          <Form.Control
            type="number"
            value={currentHP}
            className="p-0 m-0 bg-transparent border-0 shadow-none text-center flex-grow-1"
            onChange={handleCurrentHPChange}
          />
          <Form.Label
            className="p-0 pb-1 m-0 text-uppercase fw-bold"
          >
            Текущие хиты
          </Form.Label>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
}

export default CurrentHitsCard;