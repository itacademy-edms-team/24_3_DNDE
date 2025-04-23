import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import IdealsSVG from './assets/Ideals.svg';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { updateCharacterIdeals } from '../../store/sheet1Slice';

const CharacterIdeals: React.FC = () => {
  const dispatch = useAppDispatch();
  const ideals = useAppSelector((state) => state.sheet1.ideals);

  const handleChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    const newValue = event.target.value;
    dispatch(updateCharacterIdeals(newValue));
  };

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={IdealsSVG} />
      <Card.ImgOverlay className="p-2 pt-0 pb-1 m-0">
        <Form.Group
          controlId="characterIdeals"
          className="d-flex flex-column justify-content-between text-center text-truncate h-100"
        >
          <Form.Control
            as="textarea"
            value={ideals}
            className="p-1 m-0 bg-transparent border-0 shadow-none flex-grow-1"
            onChange={handleChange}
            aria-label="Идеалы"
          />
          <Form.Label className="p-0 pt-0 pb-0 m-0 text-uppercase fw-bold">
            Идеалы
          </Form.Label>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
};

export default CharacterIdeals;