﻿import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import PersonalityTraitsSVG from './assets/PersonalityTraits.svg';

import { useAppDispatch, useAppSelector } from '../../hooks';
import { selectCharacterPersonalityTraits } from '../../store/selectors/sheet1Selectors';
import { updateCharacterPersonalityTraits } from '../../store/sheet1Slice';

const PersonalityTraitsCard: React.FC = () => {
  const dispatch = useAppDispatch();
  const personalityTraits = useAppSelector(selectCharacterPersonalityTraits);

  const handleChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    const newValue = event.target.value;
    dispatch(updateCharacterPersonalityTraits(newValue));
  };

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={PersonalityTraitsSVG} />
      <Card.ImgOverlay className="p-2 pt-0 pb-1 m-0">
        <Form.Group
          controlId="characterPersonalityTraits"
          className="d-flex flex-column justify-content-between text-center text-truncate h-100"
        >
          <Form.Control
            as="textarea"
            value={personalityTraits}
            className="p-1 m-0 bg-transparent border-0 shadow-none flex-grow-1"
            onChange={handleChange}
            aria-label="Черты характера персонажа"
          />
          <Form.Label className="p-0 pt-0 pb-0 m-0 text-uppercase fw-bold">
            Черты характера
          </Form.Label>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
};

export default PersonalityTraitsCard;