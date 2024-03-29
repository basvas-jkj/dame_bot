import {get_piece, get_field} from "./board";
import {MOVE} from "./move";
import {FIELD} from "./field";
import {PIECE} from "./piece";

/* --------------------------------------------
 * | Flag enum representing static evaluation |
 * | of the position at the end of the turn.  |
 * --------------------------------------------
 */
export enum EVALUATION
{
    man_move = 1,
    wont_be_threatened = 2,
    edge_of_board = 4,
    promotion = 8,
    man_escape = 16,
    dame_escape = 32
}

/* -------------------------------------------------
 * | Checks if the field specified by arguments    |
 * | row and column is vacant (note: this function |
 * | is important for dame capturing, because this |
 * | is the only situation when piece can start    |
 * | and end a move on the same square)            |
 * -------------------------------------------------
 */
function is_field_vacant(piece: PIECE, row: number, column: number): boolean
{
    let other_piece = get_piece(row, column);
    return (other_piece == null || piece == other_piece);
}

/* ------------------------------------------------
 * | Checks if the piece given as an argument     |
 * | will be threatened by enemy on the field     |
 * | given as arguments next_row and next_column. |
 * ------------------------------------------------
 */
function will_be_threatened(piece: PIECE, next_row: number, next_column: number): boolean
{
    if (next_row == 0 || next_row == 7 || next_column == 0 || next_column == 7)
    {
        return false;
    }

    const direction = (piece.is_white) ? -1 : 1;
    
    if (is_field_vacant(piece, next_row - direction, next_column - 1))
    {
        let r = next_row + direction;
        let c = next_column + 1;
        let other_piece = get_piece(r, c);

        if (other_piece?.has_opposite_colour(piece))
        {
            return true;
        }

        try
        {
            let second_piece = true;
            let has_opposite_colour: boolean | undefined;
            if (r == piece.row && c == piece.column)
            {
                has_opposite_colour = undefined;
            }
            else
            {
                has_opposite_colour = other_piece?.has_opposite_colour(piece);
            }

            while (has_opposite_colour == undefined || (!second_piece && !has_opposite_colour) || (has_opposite_colour && !other_piece?.is_man))
            {
                if (has_opposite_colour && !other_piece?.is_man)
                {
                    return true;
                }
                else if (has_opposite_colour != undefined && !has_opposite_colour && other_piece != piece)
                {
                    second_piece = true;
                }
                else
                {
                    second_piece = false;
                }
                r += direction;
                c += 1;
                other_piece = get_piece(r, c);
                has_opposite_colour = other_piece?.has_opposite_colour(piece);
            }
        }
        catch {}
    }

    if (is_field_vacant(piece, next_row - direction, next_column + 1))
    {
        let r = next_row + direction;
        let c = next_column - 1;
        let other_piece = get_piece(r, c);

        if (other_piece?.has_opposite_colour(piece))
        {
            return true;
        }

        try
        {
            let second_piece = true;
            let has_opposite_colour: boolean | undefined;
            if (r == piece.row && c == piece.column)
            {
                has_opposite_colour = undefined;
            }
            else
            {
                has_opposite_colour = other_piece?.has_opposite_colour(piece);
            }

            while (has_opposite_colour == undefined || (!second_piece && !has_opposite_colour) || (has_opposite_colour && !other_piece?.is_man))
            {
                if (has_opposite_colour && !other_piece?.is_man)
                {
                    return true;
                }
                else if (has_opposite_colour != undefined &&!has_opposite_colour && piece != other_piece)
                {
                    second_piece = true;
                }
                else
                {
                    second_piece = false;
                }
                r += direction;
                c -= 1;
                other_piece = get_piece(r, c);
                has_opposite_colour = other_piece?.has_opposite_colour(piece);
            }
        }
        catch {}
    }

    if (is_field_vacant(piece, next_row + direction, next_column - 1))
    {
        let r = next_row - direction;
        let c = next_column + 1;
        let other_piece = get_piece(r, c);
        try
        {
            let second_piece = true;
            let has_opposite_colour: boolean | undefined;
            if (r == piece.row && c == piece.column)
            {
                has_opposite_colour = undefined;
            }
            else
            {
                has_opposite_colour = other_piece?.has_opposite_colour(piece);
            }

            while (has_opposite_colour == undefined || (!second_piece && !has_opposite_colour) || (has_opposite_colour && !other_piece?.is_man))
            {
                if (has_opposite_colour && !other_piece?.is_man)
                {
                    return true;
                }
                else if (has_opposite_colour != undefined && !has_opposite_colour && piece != other_piece)
                {
                    second_piece = true;
                }
                else
                {
                    second_piece = false;
                }
                r -= direction;
                c += 1;
                other_piece = get_piece(r, c);
                has_opposite_colour = other_piece?.has_opposite_colour(piece);
            }
        }
        catch {}
    }

    if (is_field_vacant(piece, next_row + direction, next_column + 1))
    {
        let r = next_row - direction;
        let c = next_column - 1;
        let other_piece = get_piece(r, c);

        try
        {
            let second_piece = true;
            let has_opposite_colour: boolean | undefined;
            if (r == piece.row && c == piece.column)
            {
                has_opposite_colour = undefined;
            }
            else
            {
                has_opposite_colour = other_piece?.has_opposite_colour(piece);
            }

            while (has_opposite_colour == undefined || (!second_piece && !has_opposite_colour) || (has_opposite_colour && !other_piece?.is_man))
            {
                if (has_opposite_colour && !other_piece?.is_man)
                {
                    return true;
                }
                else if (has_opposite_colour != undefined &&!has_opposite_colour && piece != other_piece)
                {
                    second_piece = true;
                }
                else
                {
                    second_piece = false;
                }
                r -= direction;
                c -= 1;
                other_piece = get_piece(r, c);
                has_opposite_colour = other_piece?.has_opposite_colour(piece);
            }
        }
        catch {}
    }

    return false;
}

/* -------------------------------
 * | Returns a static evaluation |
 * | of the position at the end  |
 * | of the turn.                |
 * -------------------------------
 */
function end_of_move_evaluation(piece: PIECE, next_row: number, next_column: number): EVALUATION
{
    let e: EVALUATION = 0;
    if (piece.is_threatened())
    {
        if (!will_be_threatened(piece, next_row, next_column))
        {
            if (!piece.is_man || (next_row == 0 || next_row == 7))
            {
                // I consider the piece which can be promoted at the end of move to be the same strong as a king.
                e |= EVALUATION.dame_escape;
            }
            else
            {
                e |= EVALUATION.man_escape;
            }

            if (next_column == 0 || next_column == 7 || next_row == 0 || next_row == 7)
            {
                e |= EVALUATION.edge_of_board;
            }
        }
    }
    else if ((next_row == 0 || next_row == 7) && piece.is_man)
    {
        e |= EVALUATION.promotion;
    }
    else if (next_column == 0 || next_column == 7 || next_row == 0 || next_row == 7)
    {
        e |= EVALUATION.edge_of_board;
    }
    else if (!will_be_threatened(piece, next_row, next_column))
    {
        e |= EVALUATION.wont_be_threatened;
    }
    else if (piece.is_man)
    {
        e |= EVALUATION.man_move;
    }
    
    return e;
}


/* ----------------------------------
 * | Checks if the king given as an |
 * | argument is able to continue   |
 * | in move with another jump.     |
 * ----------------------------------
 */
function can_king_capture(piece: PIECE, row: number, column: number, row_direction: 1 | -1, column_direction: 1 | -1, captured_pieces: PIECE[]): boolean
{
    try
    {
        let next_row = row;
        let next_column = column;
        while (true)
        {
            next_row += row_direction;
            next_column += column_direction;

            if (is_field_vacant(piece, next_row, next_column))
            {
                continue;
            }
            
            let other_piece = get_piece(next_row, next_column);
            if (other_piece!.has_opposite_colour(piece) && !captured_pieces.includes(other_piece!))
            {
                return is_field_vacant(piece, next_row + row_direction, next_column + column_direction);
            }
            else
            {
                return false;
            }
        }
    }
    catch {}
    return false;
}

/* --------------------------------------------
 * | Finds and returns all possible capturing |
 * | moves for king given as an argument in   |
 * | direction specified by parameters        |
 * | row_direction and column_direction       |
 * --------------------------------------------
 */
function *king_caputure_in_direction(piece: PIECE, row_direction: 1 | -1, column_direction: 1 | -1, fields: FIELD[] = [], captured_pieces: PIECE[] = []): Generator<MOVE, void, void>
{
    let row: number;
    let column: number;

    if (fields.length == 0)
    {
        row = piece.row;
        column = piece.column;
    }
    else
    {
        row = fields[fields.length - 1].row;
        column = fields[fields.length - 1].column;
    }

    let next_row = row + row_direction;
    let next_column = column + column_direction;
    let has_captured = false;
    let move_queue: FIELD[] = [];
    try
    {
        while (true)
        {
            let next_field = get_piece(next_row, next_column);
            if (is_field_vacant(piece, next_row, next_column))
            {
                if (has_captured)
                {
                    const field = get_field(next_row, next_column);
                    let can_capture_1 = can_king_capture(piece, next_row, next_column, row_direction, -column_direction as 1 | -1, captured_pieces);
                    let can_capture_2 = can_king_capture(piece, next_row, next_column, -row_direction as 1 | -1, column_direction, captured_pieces);
                    
                    if (can_capture_1)
                    {
                        fields.push(field);
                        yield* king_caputure_in_direction(piece, row_direction, -column_direction as 1 | -1, fields, captured_pieces);
                        fields.pop();
                    }
                    if (can_capture_2)
                    {
                        fields.push(field);
                        yield* king_caputure_in_direction(piece, -row_direction as 1 | -1, column_direction, fields, captured_pieces);
                        fields.pop();
                    }

                    if (!can_capture_1 && !can_capture_2)
                    {
                        move_queue.push(field);
                    }
                }
            }
            else if (next_field!.has_opposite_colour(piece) && is_field_vacant(piece, next_row + row_direction, next_column + column_direction))
            {
                if (has_captured)
                {
                    fields.push(get_field(next_row - row_direction, next_column - column_direction));
                    yield* king_caputure_in_direction(piece, row_direction, column_direction, fields, captured_pieces);
                    fields.pop();
                    move_queue = [];
                    break;
                }
                else
                {
                    has_captured = true;
                    captured_pieces.push(next_field!);
                }
            }
            else
            {
                break;
            }

            next_row += row_direction;
            next_column += column_direction;
        }
    }
    catch {}

    for (let move of move_queue)
    {
        let e = end_of_move_evaluation(piece, move.row, move.column);
        fields.push(move);
        yield new MOVE(piece, Object.assign([], fields), e, Object.assign([], captured_pieces));
        fields.pop();
    }
    captured_pieces.pop();
}

/* ----------------------------------
 * | Finds and returns all possible |
 * | capturing moves for king given |
 * | as an argument.                |
 * ----------------------------------
 */
export function *king_capture(piece: PIECE): Generator<MOVE, void, void>
{
    yield *king_caputure_in_direction(piece, 1, 1);
    yield *king_caputure_in_direction(piece, -1, 1);
    yield *king_caputure_in_direction(piece, 1, -1);
    yield *king_caputure_in_direction(piece, -1, -1);
}

/* ----------------------------------
 * | Finds and returns all possible |
 * | capturing moves for man given  |
 * | as an argument.                |
 * ----------------------------------
 */
export function *man_capture(piece: PIECE, direction: 1 | -1, fields: FIELD[] = [], captured_pieces: PIECE[] = []): Generator<MOVE, void, void>
{
    let jumped_row: number;
    let next_row: number;
    let column: number;

    if (fields.length == 0)
    {
        jumped_row = piece.row + direction;
        next_row = piece.row + 2 * direction;
        column = piece.column;
    }
    else
    {
        jumped_row = fields[fields.length - 1].row + direction;
        next_row = fields[fields.length - 1].row + 2 * direction;
        column = fields[fields.length - 1].column;
    }
    
    let can_jump = false;
    let jumped_field: PIECE | null;
    let next_field: PIECE | null;

    if (column < 6 && next_row >= 0 && next_row <= 7)
    {
        jumped_field = get_piece(jumped_row, column + 1);
        next_field = get_piece(next_row, column + 2);
        if (jumped_field?.has_opposite_colour(piece) && next_field == null)
        {
            can_jump = true;
            captured_pieces.push(jumped_field);
            fields.push(get_field(next_row, column + 2));
            yield* man_capture(piece, direction, fields, captured_pieces);
            fields.pop();
            captured_pieces.pop();
        }
    }
    if (column > 1 && next_row >= 0 && next_row <= 7)
    {
        jumped_field = get_piece(jumped_row, column - 1);
        next_field = get_piece(next_row, column - 2);
        if (jumped_field?.has_opposite_colour(piece) && next_field == null)
        {
            can_jump = true;
            captured_pieces.push(jumped_field);
            fields.push(get_field(next_row, column - 2));
            yield* man_capture(piece, direction, fields, captured_pieces);
            fields.pop();
            captured_pieces.pop();
        }
    }

    if (!can_jump && fields.length > 0)
    {
        let e = end_of_move_evaluation(piece, fields[fields.length - 1].row, column);
        yield new MOVE(piece, Object.assign([], fields), e, Object.assign([], captured_pieces));
    }
}

/* ---------------------------------------
 * | Finds and returns all possible      |
 * | moves for man given as an argument. |
 * ---------------------------------------
 */
export function* man_move(piece: PIECE, direction: 1 | -1): Generator<MOVE, void, void>
{
    const column = piece.column;
    const next_row = piece.row + direction;
    let next_field: PIECE | null;

    if (column < 7)
    {
        next_field = get_piece(next_row, column + 1);
        if (next_field == null)
        {
            let e = end_of_move_evaluation(piece, next_row, column + 1);
            let field = get_field(next_row, column + 1);
            yield new MOVE(piece, [field], e);
        }
    }
    if (column > 0)
    {
        next_field = get_piece(next_row, column - 1);
        if (next_field == null)
        {
            let e = end_of_move_evaluation(piece, next_row, column - 1);
            let field = get_field(next_row, column - 1);
            yield new MOVE(piece, [field], e);
        }
    }
}

/* ---------------------------------------
 * | Finds and returns all possible      |
 * | moves for man given as an argument. |
 * ---------------------------------------
 */
export function *king_move(piece: PIECE): Generator<MOVE, void, void>
{
    const row = piece.row;
    const column = piece.column;

    let next_field: PIECE | null;

    let next_row = row + 1;
    let next_column = column + 1;
    while (next_row <= 7 && next_column <= 7)
    {
        next_field = get_piece(next_row, next_column);
        if (next_field == null)
        {
            let e = end_of_move_evaluation(piece, next_row, next_column);
            let field = get_field(next_row, next_column);
            yield new MOVE(piece, [field], e);
        }
        else
        {
            break;
        }

        next_row += 1;
        next_column += 1;
    }
    
    next_row = row + 1;
    next_column = column - 1;
    while (next_row <= 7 && next_column >= 0)
    {
        next_field = get_piece(next_row, next_column);
        if (next_field == null)
        {
            let e = end_of_move_evaluation(piece, next_row, next_column);
            let field = get_field(next_row, next_column);
            yield new MOVE(piece, [field], e);
        }
        else
        {
            break;
        }

        next_row += 1;
        next_column -= 1;
    }

    next_row = row - 1;
    next_column = column + 1;
    while (next_row >= 0 && next_column <= 7)
    {
        next_field = get_piece(next_row, next_column);
        if (next_field == null)
        {
            let e = end_of_move_evaluation(piece, next_row, next_column);
            let field = get_field(next_row, next_column);
            yield new MOVE(piece, [field], e);
        }
        else
        {
            break;
        }

        next_row -= 1;
        next_column += 1;
    }

    next_row = row - 1;
    next_column = column - 1;
    while (next_row >= 0 && next_column >= 0)
    {
        next_field = get_piece(next_row, next_column);
        if (next_field == null)
        {
            let e = end_of_move_evaluation(piece, next_row, next_column);
            let field = get_field(next_row, next_column);
            yield new MOVE(piece, [field], e);
        }
        else
        {
            break;
        }

        next_row -= 1;
        next_column -= 1;
    }
}