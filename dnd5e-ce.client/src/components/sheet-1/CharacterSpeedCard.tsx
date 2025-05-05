import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import InitiativeSVG from './assets/Initiative.svg';

import { useAppDispatch, useAppSelector } from '../../hooks';
import { selectCharacterSpeed } from '../../store/selectors/sheet1Selectors';
import { updateSpeed } from '../../store/sheet1Slice';



const CharacterSpeedCard: React.FC = () => {
  const dispatch = useAppDispatch();
  const speed = useAppSelector(selectCharacterSpeed);

  const handleSpeedChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateSpeed(newValue));
    }
  };

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={InitiativeSVG} />
      <Card.ImgOverlay className="p-2 pt-0 pb-1 m-0">
        <Form.Group
          controlId="characterSpeed"
          className="d-flex flex-column justify-content-between text-center text-truncate h-100"
        >
          <Form.Control
            type="number"
            value={speed}
            className="p-0 m-0 bg-transparent border-0 shadow-none text-center flex-grow-1"
            onChange={handleSpeedChange}
          />
          <Form.Label
            className="p-0 pt-0 pb-0 m-0 text-uppercase fw-bold"
            style={{ fontSize: "0.7rem" }}
          >
            Скорость
          </Form.Label>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
}

export default CharacterSpeedCard;