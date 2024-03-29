import * as $ from "jquery";

import {PLAYER} from "./player";
import {SQUARE, FIELD} from "./field"
import {PIECE} from "./piece";

let fields: FIELD[][]; // rows × collums


//  ---------------------------------------------------------------------------------
//  |                               helping functions                               |
//  ---------------------------------------------------------------------------------

/* ------------------------------
 * | Returns field specified by |
 * | arguments row and column.  |
 * ------------------------------
 */
export function get_field(row: number, column: number): FIELD
{
    if (row < 0 || row > 7 || column < 0 || column > 7)
    {
        throw new Error();
    }

    return fields[row][column];
}

/* --------------------------------
 * | Returns field containing the |
 * | square given as an argument. |
 * --------------------------------
 */
function square_to_field(square: SQUARE): FIELD
{
    let row = FIELD.row_of_square(square);
    let column = FIELD.column_of_square(square);
    return fields[row][column];
}

/* ------------------------------
 * | Marks the piece which      |
 * | player wants to move with. |
 * ------------------------------
 */
function mark(field: FIELD): void
{
    field.square.css("background-color", "rgb(255, 0, 0)");
}

/* ----------------------------------
 * | Unmarks the piece which player |
 * | no longer wants to move with.  |
 * ----------------------------------
 */
function unmark(field: FIELD): void
{
    field?.square.css("background-color", "rgb(172, 113, 30)");
}

/* ----------------------------------
 * | Moves the piece on the board.  |
 * | Invokes the internal change of |
 * | position in the piece object.  |
 * ----------------------------------
 */
export function move_piece(previous: FIELD, next: FIELD): void
{
    const next_row = next.row;
    const next_column = next.column;

    previous.piece!.move(next_row, next_column);

    next.piece = previous.piece;
    previous.piece = null;
    previous.square.html("");
    next.square.html(next.piece!.svg);
}


//  ---------------------------------------------------------------------------------
//  |                                move validation                                |
//  ---------------------------------------------------------------------------------

let captured_pieces = Array<PIECE>(); // Stores all pieces which are going to be captured in this turn.

/* --------------------------------
 * | Represents the type of move. |
 * --------------------------------
 */
enum MOVE_TYPE
{
    possible, impossible, capturing
}

/* ----------------------------------
 * | Checks if the ongoing move is  |
 * | possible. It is also useful to |
 * | distinguish capturing from     |
 * | regular moves.                 |
 * ----------------------------------
 */
function check_move(from: FIELD, to: FIELD): MOVE_TYPE
{
    const next_row = to.row;
    const next_column = to.column;

    if (original_field != null || PLAYER.can_capture())
    {
        let captured_piece = from.piece!.is_possible_capture(next_row, next_column);
        if (captured_piece != null)
        {
            captured_pieces.push(captured_piece);
            return MOVE_TYPE.capturing;
        }
        else
        {
            return MOVE_TYPE.impossible;
        }
    }
    else if (from.piece!.is_possible_move(next_row, next_column))
    {
        return MOVE_TYPE.possible;
    }
    else
    {
        return MOVE_TYPE.impossible;
    }
}


//  --------------------------------------------------------------------------------
//  |                                 board events                                 |
//  --------------------------------------------------------------------------------

let moving = false;                // Indicates whether the player on the turn clicked on a piece which will be moving.

let previous_field: FIELD | null = null;  // Contains the last field of the piece which is moving at the moment (it change with e).
let original_field: FIELD | null = null;  // Contains the original field of the piece which is capturing at the moment.
let actual_field: FIELD | null = null;    // Contains the actual field of the piece which is capturing at the moment.

/* ---------------------------------
 * | Performs all the steps that   |
 * | accompany an interrupted turn |
 * | (such as unmarking moved      |
 * | piece and sending it back to  |
 * | the initial position).        |
 * ---------------------------------
 */
function abort_move(): void
{
    if (original_field != null)
    {
        move_piece(actual_field!, original_field!);
        captured_pieces = Array<PIECE>()
        original_field = null;
    }

    if (previous_field != null)
    {
        unmark(previous_field);
    }
    
    moving = false;
}

/* -------------------------------
 * | Performs all the steps that |
 * | accompany a successful end  |
 * | of turn (such as unmarking  |
 * | moved piece, removing       |
 * | captured pieces etc).       |
 * -------------------------------
 */
function end_move(): void
{
    unmark(previous_field!);
    moving = false;
    original_field = null;
    while (captured_pieces.length > 0)
    {
        let piece = captured_pieces.pop();
        remove_piece(piece!);
    }
    PLAYER.switch_players();
}

/* -----------------------------------
 * | If there is a marked piece and  |
 * | the clicked field is empty,     |
 * | executes the move. Otherwise,   |
 * | marks the chosen piece if there |
 * | is any on the clicked field.      |
 * -----------------------------------
 */
function black_square_clicked(this: HTMLElement): void
{
    let clicked_field = square_to_field($(this));
    
    if (moving && clicked_field.piece == null)
    {
        let move_state = check_move(previous_field!, clicked_field!);
        
        if (move_state == MOVE_TYPE.possible)
        {
            move_piece(previous_field!, clicked_field);
            if (clicked_field.piece!.can_be_promoted())
            {
                clicked_field.piece!.promote();
            }
            end_move();
        }
        else if (move_state == MOVE_TYPE.capturing)
        {
            move_piece(previous_field!, clicked_field);
            
            if (!clicked_field.piece!.is_man)
            {
                // piece on clicked field is king

                if (clicked_field.piece!.can_capture(captured_pieces))
                {
                    original_field ??= previous_field;
                    actual_field = clicked_field;

                    unmark(previous_field!);
                    mark(clicked_field);
                    previous_field = clicked_field;
                }
                else
                {
                    end_move();
                }
            }
            else if (clicked_field.piece!.can_capture())
            {
                original_field ??= previous_field;
                actual_field = clicked_field;

                unmark(previous_field!);
                mark(clicked_field);
                previous_field = clicked_field;
            }
            else
            {
                if (clicked_field.piece!.can_be_promoted())
                {
                    clicked_field.piece!.promote();
                }
                end_move();
            }
        }
    }
    else if (PLAYER.is_players_piece(clicked_field.piece!.type) && !PLAYER.is_automatic())
    {
        if (moving)
        {
            if (clicked_field == actual_field)
            {
                return;
            }
            abort_move();
        }
        mark(clicked_field);
        previous_field = clicked_field;
        moving = true;
    }
}


//  --------------------------------------------------------------------------------
//  |                           operation with gameboard                           |
//  --------------------------------------------------------------------------------

/* --------------------------------
 * | Returns piece object located |
 * | in the row and the column,   |
 * | both given as arguments.     |
 * --------------------------------
 */
export function get_piece(row: number, column: number): PIECE | null
{
    if (row > 7 || row < 0 || column > 7 || column < 0)
    {
        throw new Error();
    }
    else
    {
        return fields[row][column].piece;
    }
}

/* -------------------------------
 * | Removes the piece passed as |
 * | an argument from gameboard. |
 * -------------------------------
 */
export function remove_piece(piece: PIECE): void
{
    let field = fields[piece.row][piece.column];
    field.piece = null;
    field.square.html("");
    PLAYER.remove_piece(piece);
}

/* ---------------------------------
 * | Shows the piece passed as an  |
 * | argument in representation of |
 * | the appropriate field.        |
 * ---------------------------------
 */
export function place_piece(piece: PIECE): void
{
    let field = fields[piece.row][piece.column]
    field.square.html(piece.svg);
    field.piece = piece;
}

/* ---------------------
 * | Prepares the HTML |
 * | representation of |
 * | of the gameboard. |
 * ---------------------
 */
export function create(): void
{
    fields = new Array<FIELD[]>(8);

    for (let i = 0; i < 8; i += 1)
    {
        // i = number of row
        fields[i] = new Array<FIELD>(8);
        const row = $("<tr></tr>");
        row.append(`<td class="side_column">${(8 - i).toString()}</td>`)
        for (let j = 0; j < 8; j += 1)
        {
            // j = number of column
            const square = $(`<td id="${i}${j}"></td>`);
            square.attr("width", 100);
            square.attr("height", 100);
            square.addClass(((i + j) % 2 == 1) ? "black" : "white");
            row.append(square);
            fields[i][j] = new FIELD(square);
        }
        $("#board").append(row);
    }
    
    let side_row = $("<tr class='side_row' class ='side_column'><td></td></tr>");
    for (let i = 0; i < 8; i += 1)
    {
        side_row.append(`<td>${String.fromCharCode("a".charCodeAt(0) + i)}</td>`);
    }
    $("#board").append(side_row);

    $(".black").click(black_square_clicked);
    $(".white").click(abort_move);
    $("#board").mouseleave(abort_move);
}
