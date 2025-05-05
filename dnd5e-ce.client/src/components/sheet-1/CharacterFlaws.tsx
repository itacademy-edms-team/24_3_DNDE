import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import FlawsSVG from './assets/Flaws.svg';

import { useAppDispatch, useAppSelector } from '../../hooks';
import { selectCharacterFlaws } from '../../store/selectors/sheet1Selectors';
import { updateCharacterFlaws } from '../../store/sheet1Slice';

const CharacterFlaws: React.FC = () => {
  const dispatch = useAppDispatch();
  const flaws = useAppSelector(selectCharacterFlaws);

  const handleChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    const newValue = event.target.value;
    dispatch(updateCharacterFlaws(newValue));
  };

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={FlawsSVG} />
      <Card.ImgOverlay className="p-2 pt-0 pb-1 m-0">
        <Form.Group
          controlId="characterFlaws"
          className="d-flex flex-column justify-content-between text-center text-truncate h-100"
        >
          <Form.Control
            as="textarea"
            value={flaws}
            className="p-1 m-0 bg-transparent border-0 shadow-none flex-grow-1"
            onChange={handleChange}
            aria-label="Слабости персонажа"
          />
          <Form.Label className="p-0 pt-0 pb-0 m-0 text-uppercase fw-bold">
            Слабости
          </Form.Label>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
};

export default CharacterFlaws;